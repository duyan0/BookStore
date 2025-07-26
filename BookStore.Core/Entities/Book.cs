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
        public decimal OriginalPrice { get; set; }

        // Legacy field for backward compatibility
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price
        {
            get => FinalPrice;
            set => OriginalPrice = value;
        }

        // Discount fields
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; } // 0-100%, null means no discount

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0; // Fixed discount amount (optional)

        public bool IsOnSale { get; set; } = false;

        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        // Computed properties
        [NotMapped]
        public decimal FinalPrice
        {
            get
            {
                if (!HasActiveDiscount) return OriginalPrice;

                // Calculate discount based on percentage
                var discountFromPercentage = DiscountPercentage.HasValue
                    ? OriginalPrice * (DiscountPercentage.Value / 100)
                    : 0;

                // Add fixed discount amount if any
                var totalDiscount = discountFromPercentage + DiscountAmount;
                var finalPrice = OriginalPrice - totalDiscount;

                // Ensure price doesn't go below 0
                return Math.Max(0, finalPrice);
            }
        }

        [NotMapped]
        public bool HasActiveDiscount
        {
            get
            {
                var now = DateTime.UtcNow;
                var hasValidDiscountPercentage = DiscountPercentage.HasValue && DiscountPercentage.Value > 0;
                var hasValidDiscountAmount = DiscountAmount > 0;
                var isInSalePeriod = !IsOnSale ||
                    ((SaleStartDate == null || SaleStartDate <= now) &&
                     (SaleEndDate == null || SaleEndDate >= now));

                return (hasValidDiscountPercentage || hasValidDiscountAmount) && isInSalePeriod;
            }
        }

        [NotMapped]
        public decimal TotalSavings
        {
            get
            {
                if (!HasActiveDiscount) return 0;
                return OriginalPrice - FinalPrice;
            }
        }

        [NotMapped]
        public decimal EffectiveDiscountPercentage
        {
            get
            {
                if (!HasActiveDiscount || OriginalPrice == 0) return 0;
                return Math.Round((TotalSavings / OriginalPrice) * 100, 2);
            }
        }

        // Legacy property for backward compatibility
        [NotMapped]
        public decimal DiscountedPrice => FinalPrice;

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