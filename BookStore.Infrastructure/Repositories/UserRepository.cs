using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<IEnumerable<User>> GetAdminsAsync()
        {
            return await _dbSet
                .Where(u => u.IsAdmin)
                .ToListAsync();
        }
    }
} 