using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
    }
} 