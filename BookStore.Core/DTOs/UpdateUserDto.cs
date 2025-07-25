using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }
    }

    public class UpdateAvatarDto
    {
        [Required]
        [StringLength(255)]
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
