using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
} 