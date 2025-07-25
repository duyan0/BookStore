using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class BannerRepository : BaseRepository<Banner>, IBannerRepository
    {
        public BannerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Banners
                .Where(b => b.IsActive && 
                           (b.StartDate == null || b.StartDate <= now) &&
                           (b.EndDate == null || b.EndDate >= now))
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetBannersByPositionAsync(string position)
        {
            return await _context.Banners
                .Where(b => b.Position == position)
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetBannersByDisplayOrderAsync()
        {
            return await _context.Banners
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Banner?> GetBannerByIdAsync(int id)
        {
            return await _context.Banners
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> UpdateDisplayOrderAsync(int id, int newOrder)
        {
            var banner = await GetBannerByIdAsync(id);
            if (banner == null) return false;

            banner.DisplayOrder = newOrder;
            banner.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var banner = await GetBannerByIdAsync(id);
            if (banner == null) return false;

            banner.IsActive = !banner.IsActive;
            banner.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersByPositionAsync(string position)
        {
            var now = DateTime.UtcNow;
            return await _context.Banners
                .Where(b => b.IsActive && 
                           b.Position == position &&
                           (b.StartDate == null || b.StartDate <= now) &&
                           (b.EndDate == null || b.EndDate >= now))
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }
    }
}
