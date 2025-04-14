using AIBlog.Data;           // AppDbContext için
using AIBlog.Models;         // Post, User, Tag, vb.
using AIBlog.Interfaces;     // IRepository, IUnitOfWork, IService interface'leri
using AIBlog.Services.Repository;


public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IRepository<Post> Posts { get; }
    public IRepository<Tag> Tags { get; }
    public IRepository<User> Users { get; }
    public IRepository<Comment> Comments { get; }
    public IRepository<Category> Categories { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Posts = new Repository<Post>(_context);
        Tags = new Repository<Tag>(_context);
        Users = new Repository<User>(_context);
        Comments = new Repository<Comment>(_context);
        Categories = new Repository<Category>(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
