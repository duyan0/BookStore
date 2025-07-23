using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Core.Entities
{
    public class Book : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        [StringLength(100)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(50)]
        public string Publisher { get; set; } = string.Empty;

        public int? PublicationYear { get; set; }

        [StringLength(200)]
        public string ImageUrl { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
} 