using BookStore.Core.DTOs;
using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<IEnumerable<ReviewDto>> GetReviewsByBookIdAsync(int bookId);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(int userId);
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto);
        Task<ReviewDto?> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto);
        Task<bool> DeleteReviewAsync(int id);
        
        // Admin moderation
        Task<ReviewDto?> ModerateReviewAsync(int id, ModerateReviewDto moderateReviewDto);
        Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync();
        Task<IEnumerable<ReviewDto>> GetReviewsByStatusAsync(BookStore.Core.Entities.ReviewStatus status);
        
        // Statistics
        Task<ReviewStatisticsDto> GetReviewStatisticsAsync();
        Task<ReviewStatisticsDto> GetBookReviewStatisticsAsync(int bookId);
        
        // Helpfulness
        Task<bool> MarkReviewHelpfulAsync(int reviewId, int userId, bool isHelpful);
        Task<bool> RemoveHelpfulnessVoteAsync(int reviewId, int userId);
        
        // Verification
        Task<bool> VerifyPurchaseAsync(int reviewId, int orderId);
    }
}
