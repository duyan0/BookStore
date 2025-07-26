using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.Entities
{
    public class ReviewHelpfulness : BaseEntity
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsHelpful { get; set; } // true = helpful, false = not helpful

        // Navigation properties
        public Review Review { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
