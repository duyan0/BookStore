using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.Entities
{
    public class Author : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Biography { get; set; } = string.Empty;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
} 