using BookStore.Core.DTOs;

namespace BookStore.Web.Models
{
    public class VouchersViewModel
    {
        public List<VoucherDto> AvailableVouchers { get; set; } = new List<VoucherDto>();
        public List<VoucherDto> MyVouchers { get; set; } = new List<VoucherDto>();
        public string PageTitle { get; set; } = "Voucher";
        public bool ShowAvailable { get; set; } = false;
        public bool ShowMyVouchers { get; set; } = false;
        public string? SearchTerm { get; set; }
        public string? FilterType { get; set; }
    }

    public class VoucherValidationViewModel
    {
        public string VoucherCode { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; } = 0;
        public bool IsValid { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; } = 0;
        public bool FreeShipping { get; set; } = false;
        public string? VoucherName { get; set; }
        public string? VoucherType { get; set; }
    }
}
