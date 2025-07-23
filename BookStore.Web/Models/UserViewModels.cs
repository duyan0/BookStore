using System.ComponentModel.DataAnnotations;
using BookStore.Web.Helpers;
using BookStore.Core.DTOs;

namespace BookStore.Web.Models
{
    public class UserDashboardViewModel
    {
        public UserDto? User { get; set; }
        public List<BookViewModel> RecentBooks { get; set; } = new();
        public string WelcomeMessage { get; set; } = "";
        public int TotalBooksAvailable { get; set; }
        public int TotalCategories { get; set; }
        public int TotalAuthors { get; set; }
    }

    public class UserManagementViewModel
    {
        public List<UserDto> Users { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalUsers { get; set; } = 0;
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalUsers);
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = "";

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Họ là bắt buộc")]
        [Display(Name = "Họ")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [Display(Name = "Tên")]
        public string LastName { get; set; } = "";

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = "";

        [Display(Name = "Là Admin")]
        public bool IsAdmin { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class UserProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = "";

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Display(Name = "Họ")]
        [Required(ErrorMessage = "Vui lòng nhập họ")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
        public string FirstName { get; set; } = "";

        [Display(Name = "Tên")]
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string LastName { get; set; } = "";

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; } = "";

        [Display(Name = "Họ và tên")]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class UserOrdersViewModel
    {
        public List<OrderViewModel> Orders { get; set; } = new();
        public string Message { get; set; } = "";
    }

    public class OrderViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public List<OrderItemViewModel> Items { get; set; } = new();

        [Display(Name = "Ngày đặt hàng")]
        public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");

        [Display(Name = "Tổng tiền")]
        public string TotalAmountFormatted => CurrencyHelper.FormatVND(TotalAmount);
    }

    public class OrderItemViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;

        [Display(Name = "Thành tiền")]
        public string TotalPriceFormatted => CurrencyHelper.FormatVND(TotalPrice);
    }

    public class UserOrderViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public List<UserOrderDetailViewModel> OrderDetails { get; set; } = new List<UserOrderDetailViewModel>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");
        public string TotalAmountFormatted => CurrencyHelper.FormatVND(TotalAmount);
        public string StatusBadgeClass => Status switch
        {
            "Pending" => "bg-warning",
            "Processing" => "bg-info",
            "Completed" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
        public string StatusText => Status switch
        {
            "Pending" => "Chờ xử lý",
            "Processing" => "Đang xử lý",
            "Completed" => "Hoàn thành",
            "Cancelled" => "Đã hủy",
            _ => Status
        };
    }

    public class UserOrderDetailViewModel
    {
        public string BookTitle { get; set; } = string.Empty;
        public string BookImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        public string UnitPriceFormatted => CurrencyHelper.FormatVND(UnitPrice);
        public string TotalPriceFormatted => CurrencyHelper.FormatVND(TotalPrice);
    }
}
