using System.Linq.Expressions;

namespace AIBlog.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> AsQueryable();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity); // ← 🔁 async değil, senkron yapılmalı
        Task DeleteAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
