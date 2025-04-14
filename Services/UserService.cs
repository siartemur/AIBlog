using AIBlog.Interfaces;
using AIBlog.Models;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _unitOfWork.Users.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

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
}
