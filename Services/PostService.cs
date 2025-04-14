using AIBlog.Interfaces;
using AIBlog.Models;
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
        var allPosts = await _unitOfWork.Posts.GetAllAsync();
        return allPosts.Where(p => p.User != null && p.User.Email == email).ToList();
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync()
    {
        return await _unitOfWork.Posts.GetAllAsync();
    }

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

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        return await _unitOfWork.Posts.GetByIdAsync(id);
    }

    public async Task AddPostAsync(Post post)
    {
        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdatePostAsync(Post post)
    {
        _unitOfWork.Posts.Update(post); // async değil, EF Core zaten takip ediyor
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeletePostAsync(int id)
    {
        await _unitOfWork.Posts.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }
}
