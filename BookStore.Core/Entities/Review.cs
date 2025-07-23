using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.Entities
{
    public class Review : BaseEntity
    {
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
} 