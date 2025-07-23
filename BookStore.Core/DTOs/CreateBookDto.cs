using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class CreateBookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }

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