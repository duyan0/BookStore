using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByStatusAsync(ReviewStatus status)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.ReviewedByAdmin)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
        {
            return await GetReviewsByStatusAsync(ReviewStatus.Pending);
        }

        public async Task<Review?> GetReviewWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.ReviewedByAdmin)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Review>> GetAllReviewsWithDetailsAsync()
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.ReviewedByAdmin)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserReviewedBookAsync(int userId, int bookId)
        {
            return await _dbSet
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        public async Task<double> GetAverageRatingForBookAsync(int bookId)
        {
            var reviews = await _dbSet
                .Where(r => r.BookId == bookId && r.Status == ReviewStatus.Approved)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionForBookAsync(int bookId)
        {
            var reviews = await _dbSet
                .Where(r => r.BookId == bookId && r.Status == ReviewStatus.Approved)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var distribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                distribution[i] = reviews.FirstOrDefault(r => r.Rating == i)?.Count ?? 0;
            }

            return distribution;
        }

        public async Task<int> GetReviewCountForBookAsync(int bookId)
        {
            return await _dbSet
                .CountAsync(r => r.BookId == bookId && r.Status == ReviewStatus.Approved);
        }

        public async Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(int bookId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Order)
                .Where(r => r.BookId == bookId && r.IsVerifiedPurchase && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
