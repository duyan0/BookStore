using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
    }
}
