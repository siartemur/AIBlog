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
        public IActionResult Index(string? tag, string? category, int page = 1)
        {
            int pageSize = 6;

            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Etiket (Tag) filtresi
            if (!string.IsNullOrEmpty(tag))
                query = query.Where(p => p.Tags.Any(t => t.Text == tag));

            // ✅ Kategori filtresi
            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category != null && p.Category.Name == category);

            // Sayfalama hesaplaması
            var totalPosts = query.Count();
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            // Sayfalı post listesi
            var pagedPosts = query
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
                .ToList();

            // ViewModel içine hem kategori hem etiket bilgisi
            var viewModel = new PostListPageViewModel
            {
                Posts = pagedPosts,
                CurrentPage = page,
                TotalPages = totalPages,
                CurrentTag = tag,
                CurrentCategory = category // ✅ kategori filtresi eklendi
            };

            return View(viewModel);
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
        public async Task<IActionResult> Create(PostFormViewModel model)
        {
            var email = User.Identity?.Name;
            var user = await _userService.GetUserByEmailAsync(email!);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı.";
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name");
                return View(model);
            }

            var post = new Post
            {
                Title = model.Title,
                Description = model.Description,
                Content = model.Content,
                Url = SlugHelper.GenerateSlug(model.Title!),
                UserId = user.UserId,
                PublishedOn = DateTime.Now,
                IsActive = true,
                CategoryId = model.CategoryId
            };

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/posts");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                post.Image = "/images/posts/" + uniqueFileName;
            }

            if (!string.IsNullOrWhiteSpace(model.TagNames))
            {
                var tagTexts = model.TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doğru doldurunuz.";
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name");
                return View(model);
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

            var model = new PostFormViewModel
            {
                Title = post.Title,
                Description = post.Description,
                Content = post.Content,
                CategoryId = post.CategoryId,
                TagNames = string.Join(", ", post.Tags.Select(t => t.Text))
            };

            ViewBag.ExistingImage = post.Image;
            ViewBag.PostId = post.PostId;
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name", post.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostFormViewModel model)
        {
            var email = User.Identity?.Name;
            var user = await _userService.GetUserByEmailAsync(email!);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index", "Profile");
            }

            var post = await _context.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                TempData["ErrorMessage"] = "Güncellenecek blog bulunamadı.";
                return RedirectToAction("Index", "Profile");
            }

            post.Title = model.Title;
            post.Description = model.Description;
            post.Content = model.Content;
            post.Url = SlugHelper.GenerateSlug(model.Title!);
            post.CategoryId = model.CategoryId;
            post.IsActive = true;
            post.PublishedOn = DateTime.Now;
            post.UserId = user.UserId;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/posts");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                post.Image = "/images/posts/" + uniqueFileName;
            }

            post.Tags.Clear();

            if (!string.IsNullOrWhiteSpace(model.TagNames))
            {
                var tagTexts = model.TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var text in tagTexts)
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Text == text);
                    if (existingTag == null)
                    {
                        existingTag = new Tag { Text = text };
                        _context.Tags.Add(existingTag);
                    }

                    if (!post.Tags.Any(t => t.Text == existingTag.Text))
                    {
                        post.Tags.Add(existingTag);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "Name", model.CategoryId);
                ViewBag.ExistingImage = post.Image;
                return View(model);
            }

            await _context.SaveChangesAsync();
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
