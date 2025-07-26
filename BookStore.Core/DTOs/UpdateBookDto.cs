using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class UpdateBookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000000)]
        public decimal Price { get; set; } // This will be mapped to OriginalPrice

        // Discount fields
        [Range(0, 100)]
        public decimal? DiscountPercentage { get; set; } // 0-100%, null means no discount

        [Range(0, 10000000)]
        public decimal DiscountAmount { get; set; } = 0;

        public bool IsOnSale { get; set; } = false;

        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        [Required]
        [Range(0, 10000)]
        public int Quantity { get; set; }

        [StringLength(100)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(50)]
        public string Publisher { get; set; } = string.Empty;

        public int? PublicationYear { get; set; }

        [StringLength(200)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AuthorId { get; set; }
    }
} 