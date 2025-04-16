using AIBlog.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostService _postService;

        public HomeController(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetLatestActivePostsAsync(4);
            return View(posts);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LiveSearch(string query)
        {
            var results = await _postService.SearchPostsAsync(query, 5);
            return PartialView("_SearchResultsPartial", results);
        }
    }
}