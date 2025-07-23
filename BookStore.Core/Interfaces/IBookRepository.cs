using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(int authorId);
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
    }
} 