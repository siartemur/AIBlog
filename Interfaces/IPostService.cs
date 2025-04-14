using AIBlog.Models;

public interface IPostService
{
    Task<Post?> GetPostByUrlAsync(string url);
    Task<IEnumerable<Post>> GetPostsByUserEmailAsync(string email);
    Task<IEnumerable<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(int id);
    Task AddPostAsync(Post post);
    Task UpdatePostAsync(Post post);
    Task DeletePostAsync(int id);
}
