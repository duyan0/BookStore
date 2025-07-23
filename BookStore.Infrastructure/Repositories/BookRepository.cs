using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(int authorId)
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Title.Contains(searchTerm) ||
                           b.Description.Contains(searchTerm) ||
                           b.ISBN.Contains(searchTerm) ||
                           b.Publisher.Contains(searchTerm) ||
                           (b.Author != null && b.Author.FirstName.Contains(searchTerm)) ||
                           (b.Author != null && b.Author.LastName.Contains(searchTerm)) ||
                           (b.Category != null && b.Category.Name.Contains(searchTerm)))
                .ToListAsync();
        }

        public new async Task<Book?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public new async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();
        }
    }
} 