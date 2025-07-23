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

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(20)]
        public string PaymentMethod { get; set; } = string.Empty;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
} 