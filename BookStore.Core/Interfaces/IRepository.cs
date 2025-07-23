using BookStore.Core.Entities;
using System.Linq.Expressions;

namespace BookStore.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
} 