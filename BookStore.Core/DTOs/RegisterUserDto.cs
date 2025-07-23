using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;
    }
} 