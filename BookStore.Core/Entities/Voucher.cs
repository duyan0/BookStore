using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Core.Entities
{
    public class Voucher : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public VoucherType Type { get; set; }

        // Discount value - percentage (0-100) or fixed amount
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        // Minimum order amount to apply voucher
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumOrderAmount { get; set; } = 0;

        // Maximum discount amount (for percentage vouchers)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaximumDiscountAmount { get; set; }

        // Usage limits
        public int? UsageLimit { get; set; } // null = unlimited
        public int UsedCount { get; set; } = 0;
        public int? UsageLimitPerUser { get; set; } // null = unlimited per user

        // Validity period
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        // Computed properties
        public bool IsValid
        {
            get
            {
                var now = DateTime.UtcNow;
                return IsActive && 
                       StartDate <= now && 
                       EndDate >= now &&
                       (UsageLimit == null || UsedCount < UsageLimit);
            }
        }

        public bool IsExpired => DateTime.UtcNow > EndDate;

        public bool IsUsageLimitReached => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;

        public decimal CalculateDiscount(decimal orderAmount)
        {
            if (!IsValid || orderAmount < MinimumOrderAmount)
                return 0;

            decimal discount = Type switch
            {
                VoucherType.Percentage => orderAmount * (Value / 100),
                VoucherType.FixedAmount => Value,
                VoucherType.FreeShipping => 0, // Shipping discount handled separately
                _ => 0
            };

            // Apply maximum discount limit for percentage vouchers
            if (Type == VoucherType.Percentage && MaximumDiscountAmount.HasValue)
            {
                discount = Math.Min(discount, MaximumDiscountAmount.Value);
            }

            return Math.Min(discount, orderAmount); // Cannot exceed order amount
        }

        // Navigation properties
        public ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public enum VoucherType
    {
        Percentage = 1,
        FixedAmount = 2,
        FreeShipping = 3
    }
}
