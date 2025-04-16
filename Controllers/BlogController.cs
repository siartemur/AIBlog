using AIBlog.Interfaces;
using AIBlog.Models;
using AIBlog.ViewModels;
using AIBlog.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AIBlog.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;
        private readonly ICategoryService _categoryService;

        public BlogController(IPostService postService, IUserService userService, ICommentService commentService, ICategoryService categoryService)
        {
            _postService = postService;
            _userService = userService;
            _commentService = commentService;
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? tag, string? category, int page = 1)
        {
            var viewModel = await _postService.GetFilteredPostsAsync(tag, category, page);
            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpGet("/Category")]
        public async Task<IActionResult> Category()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View("~/Views/Category/Index.cshtml", categories);
        }

        [AllowAnonymous]
        [HttpGet("/blog/{url}")]
        public async Task<IActionResult> Details(string url)
        {
            var post = await _postService.GetPostByUrlAsync(url);
            if (post == null) return NotFound();

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
                    .ToList()
            };

            return View("Details", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string newCommentText)
        {
            var email = User.Identity?.Name;
            if (email == null) return BadRequest();

            var comment = await _commentService.AddCommentAsync(email, postId, newCommentText);
            if (comment == null) return BadRequest();

            return Json(comment);
        }

        public async Task<IActionResult> UserPosts()
        {
            var email = User.Identity?.Name;
            var posts = await _postService.GetPostsByUserEmailAsync(email!);
            return View(posts);
        }

        [HttpGet("/Blog/Create")]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostFormViewModel model)
        {
            var email = User.Identity?.Name;
            if (email == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı.";
                return RedirectToAction("Login", "Auth");
            }

            var success = await _postService.CreatePostAsync(email, model);
            if (!success)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
                TempData["ErrorMessage"] = "Blog oluşturulurken hata oluştu.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Blogunuz başarıyla oluşturuldu.";
            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _postService.GetPostFormForEditAsync(id);
            if (model == null) return NotFound();

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostFormViewModel model)
        {
            var email = User.Identity?.Name;
            var success = await _postService.UpdatePostAsync(id, email!, model);

            if (!success)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", model.CategoryId);
                TempData["ErrorMessage"] = "Blog güncellenirken hata oluştu.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Blog başarıyla güncellendi.";
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
            TempData["SuccessMessage"] = "Blog başarıyla silindi.";
            return RedirectToAction("Index", "Profile");
        }
    }
}
