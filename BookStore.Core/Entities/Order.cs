using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Core.Entities
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Voucher fields
        public int? VoucherId { get; set; }

        [StringLength(50)]
        public string? VoucherCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VoucherDiscount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 0;

        public bool FreeShipping { get; set; } = false;

        // Subtotal before voucher discount
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(20)]
        public string PaymentMethod { get; set; } = string.Empty;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public Voucher? Voucher { get; set; }
        public ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
    }
} 