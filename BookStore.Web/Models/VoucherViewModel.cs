using BookStore.Core.Entities;
using BookStore.Web.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Models
{
    public class VoucherViewModel
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
        public string TypeName => Type switch
        {
            VoucherType.Percentage => "Giảm theo phần trăm",
            VoucherType.FixedAmount => "Giảm số tiền cố định",
            VoucherType.FreeShipping => "Miễn phí vận chuyển",
            _ => "Không xác định"
        };

        public string ValueFormatted => Type switch
        {
            VoucherType.Percentage => $"{Value}%",
            VoucherType.FixedAmount => CurrencyHelper.FormatVND(Value),
            VoucherType.FreeShipping => "Miễn phí ship",
            _ => Value.ToString()
        };

        public string MinimumOrderAmountFormatted => CurrencyHelper.FormatVND(MinimumOrderAmount);
        public string MaximumDiscountAmountFormatted => MaximumDiscountAmount.HasValue ? CurrencyHelper.FormatVND(MaximumDiscountAmount.Value) : "Không giới hạn";

        public string StatusText => IsActive ? "Hoạt động" : "Không hoạt động";
        public string StatusClass => IsActive ? "success" : "secondary";

        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsUsageLimitReached => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;
        public bool IsValid => IsActive && !IsExpired && !IsUsageLimitReached;

        public string ValidityStatus
        {
            get
            {
                if (!IsActive) return "Không hoạt động";
                if (IsExpired) return "Đã hết hạn";
                if (IsUsageLimitReached) return "Đã hết lượt sử dụng";
                return "Có hiệu lực";
            }
        }

        public string ValidityStatusClass
        {
            get
            {
                if (!IsActive) return "secondary";
                if (IsExpired) return "danger";
                if (IsUsageLimitReached) return "warning";
                return "success";
            }
        }

        public int RemainingUsage => UsageLimit.HasValue ? Math.Max(0, UsageLimit.Value - UsedCount) : int.MaxValue;
        public double UsagePercentage => UsageLimit.HasValue && UsageLimit.Value > 0 ? (double)UsedCount / UsageLimit.Value * 100 : 0;
    }

    public class CreateVoucherViewModel
    {
        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Mã voucher chỉ được chứa chữ hoa và số")]
        [Display(Name = "Mã voucher")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên voucher là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên voucher không được vượt quá 200 ký tự")]
        [Display(Name = "Tên voucher")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại voucher là bắt buộc")]
        [Display(Name = "Loại voucher")]
        public VoucherType Type { get; set; }

        [Required(ErrorMessage = "Giá trị voucher là bắt buộc")]
        [Range(0.01, 999999999, ErrorMessage = "Giá trị voucher phải từ 0.01 đến 999,999,999")]
        [Display(Name = "Giá trị")]
        public decimal Value { get; set; }

        [Range(0, 999999999, ErrorMessage = "Giá trị đơn hàng tối thiểu phải từ 0 đến 999,999,999")]
        [Display(Name = "Giá trị đơn hàng tối thiểu")]
        public decimal MinimumOrderAmount { get; set; } = 0;

        [Range(0, 999999999, ErrorMessage = "Giá trị giảm tối đa phải từ 0 đến 999,999,999")]
        [Display(Name = "Giá trị giảm tối đa")]
        public decimal? MaximumDiscountAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0")]
        [Display(Name = "Giới hạn sử dụng")]
        public int? UsageLimit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng mỗi người phải lớn hơn 0")]
        [Display(Name = "Giới hạn sử dụng mỗi người")]
        public int? UsageLimitPerUser { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Display(Name = "Ngày kết thúc")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }

    public class EditVoucherViewModel : CreateVoucherViewModel
    {
        public int Id { get; set; }
        public int UsedCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Override Code to make it readonly in edit mode
        [Display(Name = "Mã voucher")]
        public new string Code { get; set; } = string.Empty;
    }

    public class VoucherApplicationViewModel
    {
        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        [Display(Name = "Mã voucher")]
        public string VoucherCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tổng tiền đơn hàng là bắt buộc")]
        public decimal OrderAmount { get; set; }

        public int UserId { get; set; }
    }

    public class VoucherValidationResultViewModel
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public bool FreeShipping { get; set; }
        public VoucherViewModel? Voucher { get; set; }

        public string DiscountAmountFormatted => CurrencyHelper.FormatVND(DiscountAmount);
    }
}
