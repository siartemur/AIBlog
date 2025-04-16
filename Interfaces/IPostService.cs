using AIBlog.Models;
using AIBlog.ViewModels;

public interface IPostService
{
    Task<Post?> GetPostByUrlAsync(string url);
    Task<IEnumerable<Post>> GetPostsByUserEmailAsync(string email);
    Task<IEnumerable<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(int id);
    Task AddPostAsync(Post post);
    Task UpdatePostAsync(Post post);
    Task DeletePostAsync(int id);

    Task<PostListPageViewModel> GetFilteredPostsAsync(string? tag, string? category, int page);
    Task<bool> CreatePostAsync(string email, PostFormViewModel model);
    Task<bool> UpdatePostAsync(int postId, string email, PostFormViewModel model);
    Task<PostFormViewModel?> GetPostFormForEditAsync(int postId);
    Task<List<PostListViewModel>> GetLatestActivePostsAsync(int count);
    Task<List<Post>> SearchPostsAsync(string query, int maxResults);
}