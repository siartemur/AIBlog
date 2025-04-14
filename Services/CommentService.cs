using AIBlog.Interfaces;
using AIBlog.Models;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
    {
        return await _unitOfWork.Comments.GetAllAsync();
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        return await _unitOfWork.Comments.FindAsync(c => c.PostId == postId);
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        return await _unitOfWork.Comments.GetByIdAsync(id);
    }

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
}
