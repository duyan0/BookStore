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

        // Discount fields
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0; // 0-100%

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0; // Fixed discount amount

        public bool IsOnSale { get; set; } = false;

        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        // Computed properties
        public decimal DiscountedPrice
        {
            get
            {
                if (!IsOnSale || !IsDiscountActive) return Price;

                var discountFromPercentage = Price * (DiscountPercentage / 100);
                var totalDiscount = discountFromPercentage + DiscountAmount;
                var finalPrice = Price - totalDiscount;

                return finalPrice < 0 ? 0 : finalPrice;
            }
        }

        public bool IsDiscountActive
        {
            get
            {
                var now = DateTime.UtcNow;
                return IsOnSale &&
                       (SaleStartDate == null || SaleStartDate <= now) &&
                       (SaleEndDate == null || SaleEndDate >= now);
            }
        }

        public decimal TotalDiscountAmount
        {
            get
            {
                if (!IsDiscountActive) return 0;
                return Price - DiscountedPrice;
            }
        }

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