// ✅ Güncellenmiş EditorController.cs
using AIBlog.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIBlog.Controllers
{
    [Authorize(Roles = "Editor")]
    public class EditorController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IUnitOfWork _unitOfWork;

        public EditorController(ICommentService commentService, IUnitOfWork unitOfWork)
        {
            _commentService = commentService;
            _unitOfWork = unitOfWork;
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
            await _unitOfWork.Comments.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction("ModerateComments");
        }
    }
}
