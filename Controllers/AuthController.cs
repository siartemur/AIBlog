using AIBlog.Interfaces;
using AIBlog.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIBlog.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null || user.Password != password)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "UserAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("UserAuth", principal);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string userName, IFormFile? ProfileImage)
        {
            var existingUser = await _userService.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Bu e-posta zaten kayıtlı.");
                return View();
            }

            string? profileImageFileName = null;

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var extension = Path.GetExtension(ProfileImage.FileName);
                var fileName = Guid.NewGuid().ToString() + extension;
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                profileImageFileName = fileName;
            }

            var newUser = new User
            {
                Email = email,
                Password = password,
                UserName = userName,
                Role = UserRole.User,
                ProfileImage = profileImageFileName
            };

            await _userService.AddUserAsync(newUser);
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserAuth");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
