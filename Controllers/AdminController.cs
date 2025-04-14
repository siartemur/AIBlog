// ✅ Güncellenmiş AdminController.cs
using AIBlog.Interfaces;
using AIBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIBlog.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUserService userService, IPostService postService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _postService = postService;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(int userId, UserRole role)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();
            user.Role = role;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _unitOfWork.Users.DeleteAsync(userId);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Posts()
        {
            var posts = await _postService.GetAllPostsAsync();
            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostStatus(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null) return NotFound();
            post.IsActive = !post.IsActive;
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction("Posts");
        }
    }
}