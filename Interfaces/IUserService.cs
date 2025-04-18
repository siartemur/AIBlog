using AIBlog.Models;

public interface IUserService
{
    Task UpdateUserAsync(User user);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task DeleteUserAsync(int userId);
    Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
}