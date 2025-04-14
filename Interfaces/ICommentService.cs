using AIBlog.Models;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetAllCommentsAsync();
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId);
    Task<Comment?> GetCommentByIdAsync(int id);
    Task AddCommentAsync(Comment comment);
    Task DeleteCommentAsync(int id);
}
