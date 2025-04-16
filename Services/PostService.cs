using AIBlog.Interfaces;
using AIBlog.Models;
using AIBlog.Helpers;
using AIBlog.ViewModels;
using Microsoft.EntityFrameworkCore;

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;

    public PostService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Post>> GetPostsByUserEmailAsync(string email)
    {
        var posts = await _unitOfWork.Posts.AsQueryable()
            .Include(p => p.User)
            .Where(p => p.User != null && p.User.Email == email)
            .ToListAsync();
        return posts;
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync() => await _unitOfWork.Posts.GetAllAsync();

    public async Task<Post?> GetPostByUrlAsync(string url)
    {
        return await _unitOfWork.Posts.AsQueryable()
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Url == url);
    }

    public async Task<Post?> GetPostByIdAsync(int id) => await _unitOfWork.Posts.GetByIdAsync(id);

    public async Task AddPostAsync(Post post)
    {
        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdatePostAsync(Post post)
    {
        _unitOfWork.Posts.Update(post);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeletePostAsync(int id)
    {
        await _unitOfWork.Posts.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<PostListPageViewModel> GetFilteredPostsAsync(string? tag, string? category, int page)
    {
        int pageSize = 6;
        var query = _unitOfWork.Posts.AsQueryable()
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(tag))
            query = query.Where(p => p.Tags.Any(t => t.Text == tag));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category != null && p.Category.Name == category);

        var totalPosts = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        var pagedPosts = await query
            .OrderByDescending(p => p.PublishedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostListViewModel
            {
                Id = p.PostId,
                Title = p.Title!,
                Description = p.Description,
                Image = p.Image,
                CategoryName = p.Category!.Name,
                AuthorName = p.User!.UserName,
                PublishedOn = p.PublishedOn,
                Url = p.Url,
                Tags = p.Tags.Select(t => t.Text!).ToList()
            })
            .ToListAsync();

        return new PostListPageViewModel
        {
            Posts = pagedPosts,
            CurrentPage = page,
            TotalPages = totalPages,
            CurrentTag = tag,
            CurrentCategory = category
        };
    }

    public async Task<bool> CreatePostAsync(string email, PostFormViewModel model)
    {
        var user = await _unitOfWork.Users.FindAsync(u => u.Email == email);
        var currentUser = user.FirstOrDefault();
        if (currentUser == null) return false;

        var post = new Post
        {
            Title = model.Title,
            Description = model.Description,
            Content = model.Content,
            Url = SlugHelper.GenerateSlug(model.Title!),
            UserId = currentUser.UserId,
            PublishedOn = DateTime.UtcNow,
            IsActive = true,
            CategoryId = model.CategoryId
        };

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/posts");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ImageFile.CopyToAsync(stream);
            post.Image = "/images/posts/" + uniqueFileName;
        }

        if (!string.IsNullOrWhiteSpace(model.TagNames))
        {
            var tags = model.TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var tag in tags)
            {
                var existingTag = await _unitOfWork.Tags.FindAsync(t => t.Text == tag);
                var tagEntity = existingTag.FirstOrDefault() ?? new Tag { Text = tag };
                if (tagEntity.TagId == 0) await _unitOfWork.Tags.AddAsync(tagEntity);
                post.Tags.Add(tagEntity);
            }
        }

        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> UpdatePostAsync(int postId, string email, PostFormViewModel model)
    {
        var post = await _unitOfWork.Posts.AsQueryable()
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.PostId == postId);
        if (post == null) return false;

        post.Title = model.Title;
        post.Description = model.Description;
        post.Content = model.Content;
        post.Url = SlugHelper.GenerateSlug(model.Title!);
        post.CategoryId = model.CategoryId;
        post.PublishedOn = DateTime.UtcNow;

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/posts");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ImageFile.CopyToAsync(stream);
            post.Image = "/images/posts/" + uniqueFileName;
        }

        post.Tags.Clear();
        if (!string.IsNullOrWhiteSpace(model.TagNames))
        {
            var tagTexts = model.TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var tagText in tagTexts)
            {
                var existing = await _unitOfWork.Tags.FindAsync(t => t.Text == tagText);
                var tagEntity = existing.FirstOrDefault() ?? new Tag { Text = tagText };
                if (tagEntity.TagId == 0) await _unitOfWork.Tags.AddAsync(tagEntity);
                post.Tags.Add(tagEntity);
            }
        }

        _unitOfWork.Posts.Update(post);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<PostFormViewModel?> GetPostFormForEditAsync(int postId)
    {
        var post = await _unitOfWork.Posts.AsQueryable()
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.PostId == postId);
        if (post == null) return null;

        return new PostFormViewModel
        {
            Title = post.Title,
            Description = post.Description,
            Content = post.Content,
            CategoryId = post.CategoryId,
            TagNames = string.Join(", ", post.Tags.Select(t => t.Text))
        };
    }

    public async Task<List<PostListViewModel>> GetLatestActivePostsAsync(int count)
    {
        return await _unitOfWork.Posts.AsQueryable()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.PublishedOn)
            .Take(count)
            .Select(p => new PostListViewModel
            {
                Id = p.PostId,
                Title = p.Title!,
                Description = p.Description,
                Image = p.Image,
                CategoryName = p.Category!.Name,
                AuthorName = p.User!.UserName,
                PublishedOn = p.PublishedOn,
                Url = p.Url
            }).ToListAsync();
    }

    public async Task<List<Post>> SearchPostsAsync(string query, int maxResults)
    {
        return await _unitOfWork.Posts.AsQueryable()
            .Include(p => p.User)
            .Where(p => p.IsActive && (
                p.Title!.Contains(query) ||
                p.Description!.Contains(query) ||
                p.Content!.Contains(query)))
            .OrderByDescending(p => p.PublishedOn)
            .Take(maxResults)
            .ToListAsync();
    }
}