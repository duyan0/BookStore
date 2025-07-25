using BookStore.Core.DTOs;

namespace BookStore.Core.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BookDto> CreateBookAsync(CreateBookDto bookDto);
        Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto bookDto);
        Task<bool> DeleteBookAsync(int id);
        Task<IEnumerable<BookDto>> GetBooksByCategoryIdAsync(int categoryId);
        Task<IEnumerable<BookDto>> GetBooksByAuthorIdAsync(int authorId);
        Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm);
        Task<BookStatisticsDto> GetBookStatisticsAsync();
    }
}