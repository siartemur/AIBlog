using Microsoft.AspNetCore.Mvc;
using AIBlog.Data;
using Microsoft.EntityFrameworkCore;
using AIBlog.ViewModels;

namespace AIBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var posts = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PublishedOn)
                .Take(4)
                .Select(p => new PostListViewModel
                {
                    Id = p.PostId,
                    Title = p.Title!,
                    Description = p.Description,
                    Image = p.Image,
                    CategoryName = p.Category.Name,
                    AuthorName = p.User.UserName,
                    PublishedOn = p.PublishedOn,
                    Url = p.Url
                })
                .ToList();

            return View(posts);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LiveSearch(string query)
        {
            var results = _context.Posts
                .Include(p => p.User)
                .Where(p => p.IsActive &&
                    (p.Title!.Contains(query) || p.Description!.Contains(query) || p.Content!.Contains(query)))
                .OrderByDescending(p => p.PublishedOn)
                .Take(5)
                .ToList();

            return PartialView("_SearchResultsPartial", results);
        }
    }
}
