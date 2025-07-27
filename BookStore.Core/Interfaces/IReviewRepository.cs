using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByBookIdAsync(int bookId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);
        Task<IEnumerable<Review>> GetReviewsByStatusAsync(ReviewStatus status);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
        Task<Review?> GetReviewWithDetailsAsync(int id);
        Task<IEnumerable<Review>> GetAllReviewsWithDetailsAsync();
        Task<bool> HasUserReviewedBookAsync(int userId, int bookId);
        Task<double> GetAverageRatingForBookAsync(int bookId);
        Task<Dictionary<int, int>> GetRatingDistributionForBookAsync(int bookId);
        Task<int> GetReviewCountForBookAsync(int bookId);
        Task<IEnumerable<Review>> GetVerifiedPurchaseReviewsAsync(int bookId);
    }
}
