using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.Entities
{
    public class Banner : BaseEntity
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(255)]
        public string LinkUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string Position { get; set; } = "home"; // home, sidebar, footer, etc.

        [StringLength(50)]
        public string Size { get; set; } = "medium"; // small, medium, large

        [StringLength(100)]
        public string ButtonText { get; set; } = string.Empty;

        [StringLength(50)]
        public string ButtonStyle { get; set; } = "primary";

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
