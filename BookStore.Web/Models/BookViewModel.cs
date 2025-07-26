using System.ComponentModel.DataAnnotations;
using BookStore.Web.Helpers;

namespace BookStore.Web.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, 10000000, ErrorMessage = "Giá phải từ 0 đến 10,000,000")]
        [Display(Name = "Giá gốc")]
        public decimal Price { get; set; }

        // Discount fields
        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0 đến 100")]
        [Display(Name = "Phần trăm giảm giá (%)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Range(0, 10000000, ErrorMessage = "Số tiền giảm phải từ 0 đến 10,000,000")]
        [Display(Name = "Số tiền giảm (VND)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "Đang khuyến mãi")]
        public bool IsOnSale { get; set; } = false;

        [Display(Name = "Ngày bắt đầu khuyến mãi")]
        public DateTime? SaleStartDate { get; set; }

        [Display(Name = "Ngày kết thúc khuyến mãi")]
        public DateTime? SaleEndDate { get; set; }

        [Display(Name = "Số lượng")]
        public int StockQuantity { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        
        [Display(Name = "Tên danh mục")]
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Tác giả là bắt buộc")]
        [Display(Name = "Tác giả")]
        public int AuthorId { get; set; }
        
        [Display(Name = "Tên tác giả")]
        public string? AuthorName { get; set; }

        [Display(Name = "Ngày xuất bản")]
        [DataType(DataType.Date)]
        public DateTime? PublishedDate { get; set; }

        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // Additional properties for shop display
        [Display(Name = "Nhà xuất bản")]
        public string Publisher { get; set; } = "";

        [Display(Name = "Năm xuất bản")]
        public int PublicationYear { get; set; }

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        // Computed properties
        [Display(Name = "Giá gốc")]
        public string PriceFormatted => CurrencyHelper.FormatVND(Price);

        public decimal DiscountedPrice
        {
            get
            {
                if (!IsOnSale || !IsDiscountActive) return Price;

                var discountFromPercentage = Price * (DiscountPercentage / 100);
                var totalDiscount = discountFromPercentage + DiscountAmount;
                var finalPrice = Price - totalDiscount;

                return finalPrice < 0 ? 0 : finalPrice;
            }
        }

        [Display(Name = "Giá khuyến mãi")]
        public string DiscountedPriceFormatted => CurrencyHelper.FormatVND(DiscountedPrice);

        public bool IsDiscountActive
        {
            get
            {
                var now = DateTime.UtcNow;
                return IsOnSale &&
                       (SaleStartDate == null || SaleStartDate <= now) &&
                       (SaleEndDate == null || SaleEndDate >= now);
            }
        }

        public decimal TotalDiscountAmount
        {
            get
            {
                if (!IsDiscountActive) return 0;
                return Price - DiscountedPrice;
            }
        }

        [Display(Name = "Tiết kiệm")]
        public string TotalDiscountAmountFormatted => CurrencyHelper.FormatVND(TotalDiscountAmount);

        public decimal DiscountPercentageDisplay
        {
            get
            {
                if (!IsDiscountActive || Price == 0) return 0;
                return Math.Round((TotalDiscountAmount / Price) * 100, 0);
            }
        }

        public bool IsInStock => Quantity > 0;
        public bool IsLowStock => Quantity > 0 && Quantity <= 5;
    }
} 