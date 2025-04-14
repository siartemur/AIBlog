using AIBlog.Data;
using AIBlog.Helpers;
using AIBlog.Interfaces;
using AIBlog.Models;
using AIBlog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AIBlog.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public BlogController(IPostService postService, IUserService userService, IUnitOfWork unitOfWork, AppDbContext context)
        {
            _postService = postService;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index(string? tag)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tag))
                query = query.Where(p => p.Tags.Any(t => t.Text == tag));

            var posts = query
                .OrderByDescending(p => p.PublishedOn)
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
                .ToList();

            ViewBag.CurrentTag = tag;
            return View(posts);
        }

        [AllowAnonymous]
        [HttpGet("/blog/{url}")]
        public async Task<IActionResult> Details(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return NotFound();

            var post = await _postService.GetPostByUrlAsync(url);

            if (post == null)
                return NotFound();

            var viewModel = new PostDetailsViewModel
            {
                Id = post.PostId,
                Title = post.Title!,
                Content = post.Content,
                Description = post.Description,
                Image = post.Image,
                PublishedOn = post.PublishedOn,
                AuthorName = post.User!.UserName!,
                CategoryName = post.Category!.Name!,
                Tags = post.Tags.Select(t => t.Text!).ToList(),
                Comments = post.Comments
                    .Where(c => c.User != null)
                    .OrderByDescending(c => c.PublishedOn)
                    .ToList() };
            return View("Details", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string newCommentText)
        {
            var email = User.Identity?.Name;
            var user = await _userService.GetUserByEmailAsync(email!);
            var post = await _context.Posts.FindAsync(postId);

            if (user == null || post == null || string.IsNullOrWhiteSpace(newCommentText))
                return BadRequest();

            var comment = new Comment
            {
                Text = newCommentText,
                PublishedOn = DateTime.Now,
                PostId = postId,
                UserId = user.UserId
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return Json(new
            {
                text = comment.Text,
                publishedOn = comment.PublishedOn,
                userName = user.UserName,
                userImage = string.IsNullOrEmpty(user.ProfileImage) ? "default.jpg" : user.ProfileImage
            });
        }


        public async Task<IActionResult> UserPosts()
        {
            var email = User.Identity?.Name;
            if (email == null) return RedirectToAction("Login", "Auth");

            var posts = await _postService.GetPostsByUserEmailAsync(email);
            return View(posts);
        }

        [HttpGet("/Blog/Create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, IFormFile? ImageFile, string? TagNames)
        {
            var email = User.Identity?.Name;
            var user = await _userService.GetUserByEmailAsync(email!);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı.";
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name");
                return View(post);
            }

            post.Url = SlugHelper.GenerateSlug(post.Title!);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/posts");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                post.Image = "/images/posts/" + uniqueFileName;
            }

            if (!string.IsNullOrWhiteSpace(TagNames))
            {
                var tagTexts = TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var text in tagTexts)
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Text == text);
                    if (existingTag == null)
                    {
                        existingTag = new Tag { Text = text };
                        _context.Tags.Add(existingTag);
                    }
                    post.Tags.Add(existingTag);
                }
            }

            post.UserId = user.UserId;
            post.PublishedOn = DateTime.Now;
            post.IsActive = true;
            post.User = null!;
            post.Category = null!;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doğru doldurunuz.";
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name");
                return View(post);
            }

            await _postService.AddPostAsync(post);
            TempData["SuccessMessage"] = "Blogunuz başarıyla oluşturuldu.";
            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name", post.CategoryId);
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Post post, IFormFile? ImageFile, string? TagNames)
        {
            var email = User.Identity?.Name;
            var user = await _userService.GetUserByEmailAsync(email!);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index", "Profile");
            }

            post.Url = SlugHelper.GenerateSlug(post.Title!);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/posts");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                post.Image = "/images/posts/" + uniqueFileName;
            }
            else
            {
                var existingPost = await _context.Posts.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PostId == post.PostId);
                if (existingPost != null)
                {
                    post.Image = existingPost.Image;
                }
            }

            // 🌟 Postu ve tag ilişkilerini güncelle (EF ile çakışmayı önler)
            var postToUpdate = await _context.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.PostId == post.PostId);

            if (postToUpdate == null)
            {
                TempData["ErrorMessage"] = "Güncellenecek blog bulunamadı.";
                return RedirectToAction("Index", "Profile");
            }

            // Temel alanları güncelle
            postToUpdate.Title = post.Title;
            postToUpdate.Description = post.Description;
            postToUpdate.Content = post.Content;
            postToUpdate.Url = post.Url;
            postToUpdate.Image = post.Image;
            postToUpdate.CategoryId = post.CategoryId;
            postToUpdate.IsActive = true;
            postToUpdate.PublishedOn = DateTime.Now;
            postToUpdate.UserId = user.UserId;

            // Etiketleri güncelle
            postToUpdate.Tags.Clear();

            if (!string.IsNullOrWhiteSpace(TagNames))
            {
                var tagTexts = TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var text in tagTexts)
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Text == text);
                    if (existingTag == null)
                    {
                        existingTag = new Tag { Text = text };
                        _context.Tags.Add(existingTag);
                    }

                    if (!postToUpdate.Tags.Any(t => t.Text == existingTag.Text))
                    {
                        postToUpdate.Tags.Add(existingTag);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name", post.CategoryId);
                return View(post);
            }

            await _context.SaveChangesAsync(); // doğrudan context üzerinden
            TempData["SuccessMessage"] = "Blogunuz başarıyla güncellendi.";
            return RedirectToAction("Index", "Profile");
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int postId)
        {
            await _postService.DeletePostAsync(postId);
            TempData["SuccessMessage"] = "Blogunuz başarıyla silindi.";
            return RedirectToAction("Index", "Profile");
        }
    }
}
