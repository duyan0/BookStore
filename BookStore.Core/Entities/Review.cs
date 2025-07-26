using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.Entities
{
    public class Review : BaseEntity
    {
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        // Moderation fields
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        [StringLength(500)]
        public string? AdminNote { get; set; }

        public int? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // Purchase verification
        public bool IsVerifiedPurchase { get; set; } = false;
        public int? OrderId { get; set; }

        // Helpfulness tracking
        public int HelpfulCount { get; set; } = 0;
        public int NotHelpfulCount { get; set; } = 0;

        // Navigation properties
        public User? ReviewedByAdmin { get; set; }
        public Order? Order { get; set; }
    }

    public enum ReviewStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Hidden = 4
    }
}