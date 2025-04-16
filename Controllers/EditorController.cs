using AIBlog.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIBlog.Controllers
{
    [Authorize(Roles = "Editor")]
    public class EditorController : Controller
    {
        private readonly ICommentService _commentService;

        public EditorController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ModerateComments");
        }

        public async Task<IActionResult> ModerateComments()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return View(comments);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _commentService.DeleteCommentAsync(id);
            return RedirectToAction("ModerateComments");
        }
    }
}