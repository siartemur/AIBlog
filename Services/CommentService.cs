using AIBlog.Interfaces;
using AIBlog.Models;
using AIBlog.ViewModels;


public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public CommentService(IUnitOfWork unitOfWork, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync() => await _unitOfWork.Comments.GetAllAsync();

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId)
        => await _unitOfWork.Comments.FindAsync(c => c.PostId == postId);

    public async Task<Comment?> GetCommentByIdAsync(int id) => await _unitOfWork.Comments.GetByIdAsync(id);

    public async Task AddCommentAsync(Comment comment)
    {
        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteCommentAsync(int id)
    {
        await _unitOfWork.Comments.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<CommentJsonViewModel?> AddCommentAsync(string email, int postId, string commentText)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null || string.IsNullOrWhiteSpace(commentText)) return null;

        var comment = new Comment
        {
            Text = commentText,
            PublishedOn = DateTime.UtcNow,
            PostId = postId,
            UserId = user.UserId
        };

        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.CompleteAsync();

        return new CommentJsonViewModel
        {
            text = comment.Text,
            publishedOn = comment.PublishedOn,
            userName = user.UserName,
            userImage = string.IsNullOrEmpty(user.ProfileImage) ? "default.jpg" : user.ProfileImage
        };
    }
}
