using AIBlog.Models;
using AIBlog.Interfaces;

public interface IUnitOfWork
{
    IRepository<Post> Posts { get; }
    IRepository<Tag> Tags { get; }
    IRepository<User> Users { get; }
    IRepository<Comment> Comments { get; }
    IRepository<Category> Categories { get; }

    Task<int> CompleteAsync();
}