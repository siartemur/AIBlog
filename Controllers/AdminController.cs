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

        public AdminController(IUserService userService, IPostService postService)
        {
            _userService = userService;
            _postService = postService;
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
            await _userService.UpdateUserAsync(user);
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            await _userService.DeleteUserAsync(userId);
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
            await _postService.UpdatePostAsync(post);
            return RedirectToAction("Posts");
        }
    }
}
