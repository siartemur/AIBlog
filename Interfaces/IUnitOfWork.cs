using AIBlog.Interfaces; // ← Bu YOKSA 'IRepository<>' görünmez!
using AIBlog.Models;     // ← Bu da yoksa 'Post', 'User' görünmez!

public interface IUnitOfWork
{
    IRepository<Post> Posts { get; }
    IRepository<Tag> Tags { get; }
    IRepository<User> Users { get; }
    IRepository<Comment> Comments { get; }
    IRepository<Category> Categories { get; }

    Task<int> CompleteAsync(); // SaveChanges burada
}
