using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Models
{
    public class OtpVerificationViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải là 6 chữ số")]
        public string OtpCode { get; set; } = string.Empty;

        public int RemainingAttempts { get; set; } = 5;
        public bool CanResend { get; set; } = true;
        public DateTime? ExpiryTime { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class ResendOtpRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
}
