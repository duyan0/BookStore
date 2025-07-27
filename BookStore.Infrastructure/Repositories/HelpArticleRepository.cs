using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class HelpArticleRepository : BaseRepository<HelpArticle>, IHelpArticleRepository
    {
        public HelpArticleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<HelpArticle?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .Include(h => h.Author)
                .Include(h => h.LastModifiedBy)
                .FirstOrDefaultAsync(h => h.Slug == slug && h.IsPublished);
        }

        public async Task<IEnumerable<HelpArticle>> GetByCategoryAsync(HelpArticleCategory category)
        {
            return await _dbSet
                .Include(h => h.Author)
                .Where(h => h.Category == category && h.IsPublished)
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HelpArticle>> GetByTypeAsync(HelpArticleType type)
        {
            return await _dbSet
                .Include(h => h.Author)
                .Where(h => h.Type == type && h.IsPublished)
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HelpArticle>> GetFeaturedArticlesAsync()
        {
            return await _dbSet
                .Include(h => h.Author)
                .Where(h => h.IsFeatured && h.IsPublished)
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HelpArticle>> GetPublishedArticlesAsync()
        {
            return await _dbSet
                .Include(h => h.Author)
                .Where(h => h.IsPublished)
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HelpArticle>> GetRecentArticlesAsync(int count)
        {
            return await _dbSet
                .Include(h => h.Author)
                .Where(h => h.IsPublished)
                .OrderByDescending(h => h.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<HelpArticle>> SearchArticlesAsync(string searchTerm)
        {
            return await _dbSet
                .Include(h => h.Author)
                .Where(h => h.IsPublished && 
                           (h.Title.Contains(searchTerm) || 
                            h.Content.Contains(searchTerm) || 
                            h.Summary.Contains(searchTerm)))
                .OrderByDescending(h => h.ViewCount)
                .ThenByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HelpArticle>> SearchArticlesAsync(string searchTerm, HelpArticleType? type, HelpArticleCategory? category)
        {
            var query = _dbSet
                .Include(h => h.Author)
                .Where(h => h.IsPublished);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(h => h.Title.Contains(searchTerm) || 
                                        h.Content.Contains(searchTerm) || 
                                        h.Summary.Contains(searchTerm));
            }

            if (type.HasValue)
            {
                query = query.Where(h => h.Type == type.Value);
            }

            if (category.HasValue)
            {
                query = query.Where(h => h.Category == category.Value);
            }

            return await query
                .OrderByDescending(h => h.ViewCount)
                .ThenByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
        {
            var query = _dbSet.Where(h => h.Slug == slug);
            
            if (excludeId.HasValue)
            {
                query = query.Where(h => h.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var article = await _dbSet.FindAsync(id);
            if (article == null) return false;

            article.ViewCount++;
            article.LastViewedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
