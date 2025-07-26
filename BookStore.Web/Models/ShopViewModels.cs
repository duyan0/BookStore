using System.ComponentModel.DataAnnotations;
using BookStore.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Web.Models
{
    public class ShopViewModel
    {
        public List<BookViewModel> Books { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        
        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalBooks { get; set; } = 0;
        public bool HasPreviousPage { get; set; } = false;
        public bool HasNextPage { get; set; } = false;
        
        // Filtering
        public string SearchTerm { get; set; } = "";
        public int? SelectedCategoryId { get; set; }
        
        // Display helpers
        public string PaginationInfo => $"Hiển thị {((CurrentPage - 1) * PageSize) + 1}-{Math.Min(CurrentPage * PageSize, TotalBooks)} trong tổng số {TotalBooks} sách";
    }

    public class BookDetailsViewModel
    {
        public BookViewModel Book { get; set; } = new();
        public List<BookViewModel> RelatedBooks { get; set; } = new();
        
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int Quantity { get; set; } = 1;
        
        public bool IsInStock => Book.Quantity > 0;
        public bool CanAddToCart => IsInStock && Quantity <= Book.Quantity;
    }

    public class CartViewModel
    {
        public List<CartItemDetailViewModel> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
        public decimal TotalOriginalAmount => Items.Sum(i => i.BookPrice * i.Quantity);
        public decimal TotalSavings => Items.Sum(i => i.TotalSavings);
        public int TotalItems => Items.Sum(i => i.Quantity);
        public bool IsEmpty => !Items.Any();
        public bool HasDiscounts => Items.Any(i => i.IsDiscountActive);

        [Display(Name = "Tổng tiền")]
        public string TotalAmountFormatted => CurrencyHelper.FormatVND(TotalAmount);

        [Display(Name = "Tổng tiền gốc")]
        public string TotalOriginalAmountFormatted => CurrencyHelper.FormatVND(TotalOriginalAmount);

        [Display(Name = "Tổng tiết kiệm")]
        public string TotalSavingsFormatted => CurrencyHelper.FormatVND(TotalSavings);

        [Display(Name = "Tổng số lượng")]
        public string TotalItemsText => TotalItems == 1 ? "1 sản phẩm" : $"{TotalItems} sản phẩm";
    }

    public class CartItemViewModel
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemDetailViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public decimal BookPrice { get; set; }
        public string BookImageUrl { get; set; } = "";
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }

        // Discount fields
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public bool IsOnSale { get; set; } = false;
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public decimal DiscountedPrice { get; set; }
        public bool IsDiscountActive { get; set; }

        // Use discounted price for calculations
        public decimal EffectivePrice => IsDiscountActive ? DiscountedPrice : BookPrice;
        public decimal TotalPrice => EffectivePrice * Quantity;
        public decimal TotalSavings => IsDiscountActive ? (BookPrice - DiscountedPrice) * Quantity : 0;

        [Display(Name = "Giá gốc")]
        public string BookPriceFormatted => CurrencyHelper.FormatVND(BookPrice);

        [Display(Name = "Đơn giá")]
        public string EffectivePriceFormatted => CurrencyHelper.FormatVND(EffectivePrice);

        [Display(Name = "Giá khuyến mãi")]
        public string DiscountedPriceFormatted => CurrencyHelper.FormatVND(DiscountedPrice);

        [Display(Name = "Thành tiền")]
        public string TotalPriceFormatted => CurrencyHelper.FormatVND(TotalPrice);

        [Display(Name = "Tiết kiệm")]
        public string TotalSavingsFormatted => CurrencyHelper.FormatVND(TotalSavings);

        public decimal DiscountPercentageDisplay
        {
            get
            {
                if (!IsDiscountActive || BookPrice == 0) return 0;
                return Math.Round(((BookPrice - DiscountedPrice) / BookPrice) * 100, 0);
            }
        }

        public bool IsOutOfStock => MaxQuantity <= 0;
        public bool ExceedsStock => Quantity > MaxQuantity;
    }



    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // Voucher fields
        [Display(Name = "Mã voucher")]
        public string? VoucherCode { get; set; }

        public bool HasVoucher => !string.IsNullOrEmpty(VoucherCode) && VoucherDiscount > 0;
        public decimal VoucherDiscount { get; set; } = 0;
        public bool VoucherFreeShipping { get; set; } = false;
        public string VoucherMessage { get; set; } = string.Empty;

        public List<CartItemDetailViewModel> Items { get; set; } = new List<CartItemDetailViewModel>();

        // Computed properties
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
        public decimal ShippingFee => (TotalAmount >= 500000 || VoucherFreeShipping) ? 0 : 30000; // Free shipping for orders over 500k VND or with voucher
        public decimal DiscountAmount => VoucherDiscount;
        public decimal FinalAmount => Math.Max(0, TotalAmount - DiscountAmount + ShippingFee);

        // Formatted properties
        [Display(Name = "Tổng tiền hàng")]
        public string TotalAmountFormatted => CurrencyHelper.FormatVND(TotalAmount);

        [Display(Name = "Phí vận chuyển")]
        public string ShippingFeeFormatted => CurrencyHelper.FormatVND(ShippingFee);

        [Display(Name = "Giảm giá voucher")]
        public string VoucherDiscountFormatted => CurrencyHelper.FormatVND(VoucherDiscount);

        [Display(Name = "Tổng thanh toán")]
        public string FinalAmountFormatted => CurrencyHelper.FormatVND(FinalAmount);

        public static List<SelectListItem> PaymentMethods => new List<SelectListItem>
        {
            new SelectListItem { Value = "COD", Text = "Thanh toán khi nhận hàng (COD)" },
            new SelectListItem { Value = "BankTransfer", Text = "Chuyển khoản ngân hàng" },
            new SelectListItem { Value = "CreditCard", Text = "Thẻ tín dụng" },
            new SelectListItem { Value = "EWallet", Text = "Ví điện tử" }
        };
    }

    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng (COD)")]
        CashOnDelivery = 1,

        [Display(Name = "Chuyển khoản ngân hàng")]
        BankTransfer = 2,

        [Display(Name = "Ví điện tử")]
        EWallet = 3
    }
}
