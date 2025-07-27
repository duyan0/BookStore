using BookStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public ReviewStatus Status { get; set; }
        public string? AdminNote { get; set; }
        public int? ReviewedByAdminId { get; set; }
        public string? ReviewedByAdminName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public int? OrderId { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public string StatusText => Status switch
        {
            ReviewStatus.Pending => "Chờ duyệt",
            ReviewStatus.Approved => "Đã duyệt",
            ReviewStatus.Rejected => "Bị từ chối",
            ReviewStatus.Hidden => "Đã ẩn",
            _ => "Không xác định"
        };

        public string RatingStars => new string('★', Rating) + new string('☆', 5 - Rating);
        public int TotalVotes => HelpfulCount + NotHelpfulCount;
        public double HelpfulPercentage => TotalVotes > 0 ? (double)HelpfulCount / TotalVotes * 100 : 0;
    }

    public class CreateReviewDto
    {
        [Required(ErrorMessage = "ID sách là bắt buộc")]
        public int BookId { get; set; }

        public int UserId { get; set; } // Will be set from authentication context

        [Required(ErrorMessage = "Đánh giá sao là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        public string Comment { get; set; } = string.Empty;

        public int? OrderId { get; set; } // For purchase verification
    }

    public class UpdateReviewDto
    {
        [Required(ErrorMessage = "Đánh giá sao là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        public string Comment { get; set; } = string.Empty;
    }

    public class ModerateReviewDto
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public ReviewStatus Status { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú admin không được vượt quá 500 ký tự")]
        public string? AdminNote { get; set; }
    }

    public class ReviewHelpfulnessDto
    {
        [Required(ErrorMessage = "ID đánh giá là bắt buộc")]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Đánh giá hữu ích là bắt buộc")]
        public bool IsHelpful { get; set; }
    }

    public class BookReviewSummaryDto
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public int VerifiedPurchaseCount { get; set; }

        // Computed properties
        public string AverageRatingFormatted => AverageRating.ToString("F1");
        public string AverageRatingStars => new string('★', (int)Math.Round(AverageRating)) + 
                                          new string('☆', 5 - (int)Math.Round(AverageRating));
        
        public double FiveStarPercentage => TotalReviews > 0 ? (double)FiveStarCount / TotalReviews * 100 : 0;
        public double FourStarPercentage => TotalReviews > 0 ? (double)FourStarCount / TotalReviews * 100 : 0;
        public double ThreeStarPercentage => TotalReviews > 0 ? (double)ThreeStarCount / TotalReviews * 100 : 0;
        public double TwoStarPercentage => TotalReviews > 0 ? (double)TwoStarCount / TotalReviews * 100 : 0;
        public double OneStarPercentage => TotalReviews > 0 ? (double)OneStarCount / TotalReviews * 100 : 0;
        public double VerifiedPurchasePercentage => TotalReviews > 0 ? (double)VerifiedPurchaseCount / TotalReviews * 100 : 0;
    }

    public class ReviewStatisticsDto
    {
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int ApprovedReviews { get; set; }
        public int RejectedReviews { get; set; }
        public int HiddenReviews { get; set; }
        public int VerifiedPurchaseReviews { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsThisMonth { get; set; }
        public int ReviewsThisWeek { get; set; }
        public DateTime LastUpdated { get; set; }

        // Top reviewed books
        public List<BookReviewSummaryDto> TopReviewedBooks { get; set; } = new List<BookReviewSummaryDto>();
    }
}
