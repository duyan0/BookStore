using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class SliderRepository : BaseRepository<Slider>, ISliderRepository
    {
        public SliderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Slider>> GetActiveSlidersAsync()
        {
            return await _context.Sliders
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Slider>> GetSlidersByDisplayOrderAsync()
        {
            return await _context.Sliders
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Slider?> GetSliderByIdAsync(int id)
        {
            return await _context.Sliders
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> UpdateDisplayOrderAsync(int id, int newOrder)
        {
            var slider = await GetSliderByIdAsync(id);
            if (slider == null) return false;

            slider.DisplayOrder = newOrder;
            slider.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var slider = await GetSliderByIdAsync(id);
            if (slider == null) return false;

            slider.IsActive = !slider.IsActive;
            slider.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
