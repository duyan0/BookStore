using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class CreateAuthorDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Biography { get; set; } = string.Empty;
    }
} 