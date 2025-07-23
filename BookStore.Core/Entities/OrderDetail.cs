using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Core.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
} 