using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<IEnumerable<Author>> SearchAuthorsAsync(string searchTerm);
    }
} 