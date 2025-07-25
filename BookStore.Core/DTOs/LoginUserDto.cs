using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class LoginUserDto
    {
        [Required]
        [Display(Name = "Email hoặc Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
} 