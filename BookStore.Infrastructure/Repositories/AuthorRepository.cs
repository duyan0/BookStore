using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class AuthorRepository : BaseRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Author>> SearchAuthorsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(a => a.FirstName.Contains(searchTerm) || 
                           a.LastName.Contains(searchTerm) || 
                           a.Biography.Contains(searchTerm))
                .ToListAsync();
        }

        public new async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.Books)
                .ToListAsync();
        }

        public new async Task<Author?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
} 