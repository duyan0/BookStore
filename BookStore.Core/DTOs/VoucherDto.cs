using BookStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class VoucherDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public decimal Value { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public int? UsageLimitPerUser { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public bool IsUsageLimitReached { get; set; }
        public string TypeName => Type.ToString();
        public string StatusText => IsValid ? "Hoạt động" : IsExpired ? "Hết hạn" : IsUsageLimitReached ? "Hết lượt sử dụng" : "Không hoạt động";
    }

    public class CreateVoucherDto
    {
        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Mã voucher chỉ được chứa chữ hoa và số")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên voucher là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên voucher không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại voucher là bắt buộc")]
        public VoucherType Type { get; set; }

        [Required(ErrorMessage = "Giá trị voucher là bắt buộc")]
        [Range(0.01, 999999999, ErrorMessage = "Giá trị voucher phải từ 0.01 đến 999,999,999")]
        public decimal Value { get; set; }

        [Range(0, 999999999, ErrorMessage = "Giá trị đơn hàng tối thiểu phải từ 0 đến 999,999,999")]
        public decimal MinimumOrderAmount { get; set; } = 0;

        [Range(0, 999999999, ErrorMessage = "Giá trị giảm tối đa phải từ 0 đến 999,999,999")]
        public decimal? MaximumDiscountAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0")]
        public int? UsageLimit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng mỗi người phải lớn hơn 0")]
        public int? UsageLimitPerUser { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateVoucherDto : CreateVoucherDto
    {
        // Inherits all properties from CreateVoucherDto
        // Code cannot be updated after creation for security
        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự")]
        public new string Code { get; set; } = string.Empty;
    }

    public class VoucherValidationDto
    {
        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tổng tiền đơn hàng là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền đơn hàng phải lớn hơn 0")]
        public decimal OrderAmount { get; set; }

        public int? UserId { get; set; }
    }

    public class VoucherValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public bool FreeShipping { get; set; }
        public VoucherDto? Voucher { get; set; }
    }

    public class ApplyVoucherDto
    {
        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        public string VoucherCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tổng tiền đơn hàng là bắt buộc")]
        public decimal OrderAmount { get; set; }

        [Required(ErrorMessage = "ID người dùng là bắt buộc")]
        public int UserId { get; set; }
    }

    public class VoucherUsageDto
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public string VoucherName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal OrderAmount { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
