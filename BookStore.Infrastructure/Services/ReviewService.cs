using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public ReviewService(IReviewRepository reviewRepository, IMapper mapper, ApplicationDbContext context)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAllReviewsWithDetailsAsync();
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByBookIdAsync(int bookId)
        {
            var reviews = await _reviewRepository.GetReviewsByBookIdAsync(bookId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(int userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetReviewWithDetailsAsync(id);
            return review == null ? null : _mapper.Map<ReviewDto>(review);
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            // Check if user has already reviewed this book
            var hasReviewed = await _reviewRepository.HasUserReviewedBookAsync(createReviewDto.UserId, createReviewDto.BookId);
            if (hasReviewed)
            {
                throw new InvalidOperationException("Bạn đã đánh giá sách này rồi.");
            }

            var review = _mapper.Map<Review>(createReviewDto);
            review.Status = ReviewStatus.Pending; // Default to pending for moderation
            
            // Check if this is a verified purchase
            var hasOrderedBook = await _context.OrderDetails
                .Include(od => od.Order)
                .AnyAsync(od => od.BookId == createReviewDto.BookId && 
                               od.Order.UserId == createReviewDto.UserId &&
                               od.Order.Status == "Completed");
            
            review.IsVerifiedPurchase = hasOrderedBook;

            var createdReview = await _reviewRepository.AddAsync(review);
            var reviewWithDetails = await _reviewRepository.GetReviewWithDetailsAsync(createdReview.Id);
            
            return _mapper.Map<ReviewDto>(reviewWithDetails);
        }

        public async Task<ReviewDto?> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) return null;

            review.Rating = updateReviewDto.Rating;
            review.Comment = updateReviewDto.Comment;
            review.Status = ReviewStatus.Pending; // Reset to pending after edit

            await _reviewRepository.UpdateAsync(review);
            
            var updatedReview = await _reviewRepository.GetReviewWithDetailsAsync(id);
            return _mapper.Map<ReviewDto>(updatedReview);
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) return false;

            await _reviewRepository.DeleteAsync(review);
            return true;
        }

        public async Task<ReviewDto?> ModerateReviewAsync(int id, ModerateReviewDto moderateReviewDto)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) return null;

            review.Status = moderateReviewDto.Status;
            review.AdminNote = moderateReviewDto.AdminNote;
            review.ReviewedAt = DateTime.UtcNow;
            // Note: ReviewedByAdminId should be set from the current admin user context

            await _reviewRepository.UpdateAsync(review);
            
            var moderatedReview = await _reviewRepository.GetReviewWithDetailsAsync(id);
            return _mapper.Map<ReviewDto>(moderatedReview);
        }

        public async Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync()
        {
            var reviews = await _reviewRepository.GetPendingReviewsAsync();
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByStatusAsync(ReviewStatus status)
        {
            var reviews = await _reviewRepository.GetReviewsByStatusAsync(status);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync()
        {
            var allReviews = await _context.Reviews.ToListAsync();
            
            return new ReviewStatisticsDto
            {
                TotalReviews = allReviews.Count,
                PendingReviews = allReviews.Count(r => r.Status == ReviewStatus.Pending),
                ApprovedReviews = allReviews.Count(r => r.Status == ReviewStatus.Approved),
                RejectedReviews = allReviews.Count(r => r.Status == ReviewStatus.Rejected),
                HiddenReviews = allReviews.Count(r => r.Status == ReviewStatus.Hidden),
                VerifiedPurchaseReviews = allReviews.Count(r => r.IsVerifiedPurchase),
                AverageRating = allReviews.Any() ? allReviews.Where(r => r.Status == ReviewStatus.Approved).Average(r => r.Rating) : 0,
                ReviewsThisMonth = allReviews.Count(r => r.CreatedAt.Month == DateTime.Now.Month && r.CreatedAt.Year == DateTime.Now.Year),
                ReviewsThisWeek = allReviews.Count(r => r.CreatedAt >= DateTime.Now.AddDays(-7))
            };
        }

        public async Task<ReviewStatisticsDto> GetBookReviewStatisticsAsync(int bookId)
        {
            var bookReviews = await _context.Reviews.Where(r => r.BookId == bookId).ToListAsync();
            
            return new ReviewStatisticsDto
            {
                TotalReviews = bookReviews.Count,
                PendingReviews = bookReviews.Count(r => r.Status == ReviewStatus.Pending),
                ApprovedReviews = bookReviews.Count(r => r.Status == ReviewStatus.Approved),
                RejectedReviews = bookReviews.Count(r => r.Status == ReviewStatus.Rejected),
                HiddenReviews = bookReviews.Count(r => r.Status == ReviewStatus.Hidden),
                VerifiedPurchaseReviews = bookReviews.Count(r => r.IsVerifiedPurchase),
                AverageRating = bookReviews.Any() ? bookReviews.Where(r => r.Status == ReviewStatus.Approved).Average(r => r.Rating) : 0,
                ReviewsThisMonth = bookReviews.Count(r => r.CreatedAt.Month == DateTime.Now.Month && r.CreatedAt.Year == DateTime.Now.Year),
                ReviewsThisWeek = bookReviews.Count(r => r.CreatedAt >= DateTime.Now.AddDays(-7))
            };
        }

        public async Task<bool> MarkReviewHelpfulAsync(int reviewId, int userId, bool isHelpful)
        {
            // Check if user has already voted
            var existingVote = await _context.ReviewHelpfulness
                .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId);

            if (existingVote != null)
            {
                // Update existing vote
                existingVote.IsHelpful = isHelpful;
            }
            else
            {
                // Create new vote
                var newVote = new ReviewHelpfulness
                {
                    ReviewId = reviewId,
                    UserId = userId,
                    IsHelpful = isHelpful
                };
                _context.ReviewHelpfulness.Add(newVote);
            }

            // Update review counts
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review != null)
            {
                var helpfulCount = await _context.ReviewHelpfulness
                    .CountAsync(rh => rh.ReviewId == reviewId && rh.IsHelpful);
                var notHelpfulCount = await _context.ReviewHelpfulness
                    .CountAsync(rh => rh.ReviewId == reviewId && !rh.IsHelpful);

                review.HelpfulCount = helpfulCount;
                review.NotHelpfulCount = notHelpfulCount;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveHelpfulnessVoteAsync(int reviewId, int userId)
        {
            var vote = await _context.ReviewHelpfulness
                .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId);

            if (vote == null) return false;

            _context.ReviewHelpfulness.Remove(vote);

            // Update review counts
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review != null)
            {
                var helpfulCount = await _context.ReviewHelpfulness
                    .CountAsync(rh => rh.ReviewId == reviewId && rh.IsHelpful);
                var notHelpfulCount = await _context.ReviewHelpfulness
                    .CountAsync(rh => rh.ReviewId == reviewId && !rh.IsHelpful);

                review.HelpfulCount = helpfulCount;
                review.NotHelpfulCount = notHelpfulCount;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyPurchaseAsync(int reviewId, int orderId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            // Verify that the order exists and contains the book
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.Order.Id == orderId && 
                                          od.BookId == review.BookId && 
                                          od.Order.UserId == review.UserId);

            if (orderDetail == null) return false;

            review.IsVerifiedPurchase = true;
            review.OrderId = orderId;

            await _reviewRepository.UpdateAsync(review);
            return true;
        }
    }
}
