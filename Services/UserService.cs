using AIBlog.Interfaces;
using AIBlog.Models;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    public UserService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<User>> GetAllUsersAsync() => await _unitOfWork.Users.GetAllAsync();

    public async Task<User?> GetUserByIdAsync(int id) => await _unitOfWork.Users.GetByIdAsync(id);

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email == email);
        return users.FirstOrDefault();
    }

    public async Task AddUserAsync(User user)
    {
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        // Yorumları sil
        var comments = await _unitOfWork.Comments.FindAsync(c => c.UserId == userId);
        foreach (var comment in comments)
        {
            await _unitOfWork.Comments.DeleteAsync(comment.CommentId);
        }

        // Blog yazılarını sil
        var posts = await _unitOfWork.Posts.FindAsync(p => p.UserId == userId);
        foreach (var post in posts)
        {
            await _unitOfWork.Posts.DeleteAsync(post.PostId);
        }

        // Kullanıcıyı sil
        await _unitOfWork.Users.DeleteAsync(userId);

        await _unitOfWork.CompleteAsync();
    }

    public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null || user.Password != currentPassword) return false;

        user.Password = newPassword;
        await UpdateUserAsync(user);
        return true;
    }
}
