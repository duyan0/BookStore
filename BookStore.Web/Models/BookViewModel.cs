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
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

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
        [Display(Name = "Giá")]
        public string PriceFormatted => CurrencyHelper.FormatVND(Price);

        public bool IsInStock => Quantity > 0;
        public bool IsLowStock => Quantity > 0 && Quantity <= 5;
    }
} 