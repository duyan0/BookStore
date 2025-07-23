using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public new async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Books)
                .ToListAsync();
        }

        public new async Task<Category?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
} 