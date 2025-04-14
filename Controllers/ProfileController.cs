using AIBlog.Interfaces;
using AIBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIBlog.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;

        public ProfileController(IUserService userService, IPostService postService)
        {
            _userService = userService;
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var email = User.Identity?.Name;
            if (email == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var posts = await _postService.GetAllPostsAsync();
            var myPosts = posts.Where(p => p.UserId == user.UserId).ToList();

            var model = (user, myPosts);
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Yeni şifreler uyuşmuyor.");
                return View();
            }

            var email = User.Identity?.Name;
            if (email == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null || user.Password != currentPassword)
            {
                ModelState.AddModelError("", "Mevcut şifre hatalı.");
                return View();
            }

            user.Password = newPassword;
            await _userService.UpdateUserAsync(user);

            TempData["SuccessMessage"] = "Şifre başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
    }
}
