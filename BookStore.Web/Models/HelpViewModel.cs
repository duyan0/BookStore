using BookStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Models
{
    public class HelpCenterHomeViewModel
    {
        public List<HelpArticleListViewModel> FeaturedArticles { get; set; } = new List<HelpArticleListViewModel>();
        public List<HelpArticleListViewModel> RecentArticles { get; set; } = new List<HelpArticleListViewModel>();
        public List<HelpArticleListViewModel> PopularFAQs { get; set; } = new List<HelpArticleListViewModel>();
        public List<HelpCategoryStatsViewModel> Categories { get; set; } = new List<HelpCategoryStatsViewModel>();
    }

    public class HelpArticleViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public HelpArticleType Type { get; set; }
        public HelpArticleCategory Category { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;

        // Computed properties
        public string TypeName => Type switch
        {
            HelpArticleType.FAQ => "Câu hỏi thường gặp",
            HelpArticleType.Policy => "Chính sách",
            HelpArticleType.Guide => "Hướng dẫn",
            HelpArticleType.Tutorial => "Hướng dẫn chi tiết",
            HelpArticleType.Announcement => "Thông báo",
            _ => "Khác"
        };

        public string CategoryName => Category switch
        {
            HelpArticleCategory.General => "Tổng quan",
            HelpArticleCategory.Account => "Tài khoản",
            HelpArticleCategory.Orders => "Đơn hàng",
            HelpArticleCategory.Payment => "Thanh toán",
            HelpArticleCategory.Shipping => "Vận chuyển",
            HelpArticleCategory.Returns => "Đổi trả",
            HelpArticleCategory.Technical => "Kỹ thuật",
            HelpArticleCategory.Privacy => "Bảo mật",
            HelpArticleCategory.Terms => "Điều khoản",
            HelpArticleCategory.Contact => "Liên hệ",
            _ => "Khác"
        };

        public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy");
        public string UpdatedAtFormatted => UpdatedAt?.ToString("dd/MM/yyyy") ?? "";
    }

    public class HelpArticleListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public HelpArticleType Type { get; set; }
        public HelpArticleCategory Category { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
        public string TypeName => Type switch
        {
            HelpArticleType.FAQ => "FAQ",
            HelpArticleType.Policy => "Chính sách",
            HelpArticleType.Guide => "Hướng dẫn",
            HelpArticleType.Tutorial => "Tutorial",
            HelpArticleType.Announcement => "Thông báo",
            _ => "Khác"
        };

        public string CategoryName => Category switch
        {
            HelpArticleCategory.General => "Tổng quan",
            HelpArticleCategory.Account => "Tài khoản",
            HelpArticleCategory.Orders => "Đơn hàng",
            HelpArticleCategory.Payment => "Thanh toán",
            HelpArticleCategory.Shipping => "Vận chuyển",
            HelpArticleCategory.Returns => "Đổi trả",
            HelpArticleCategory.Technical => "Kỹ thuật",
            HelpArticleCategory.Privacy => "Bảo mật",
            HelpArticleCategory.Terms => "Điều khoản",
            HelpArticleCategory.Contact => "Liên hệ",
            _ => "Khác"
        };

        public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy");
    }

    public class HelpCategoryViewModel
    {
        public HelpArticleCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<HelpArticleListViewModel> Articles { get; set; } = new List<HelpArticleListViewModel>();
    }

    public class HelpCategoryStatsViewModel
    {
        public HelpArticleCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public string IconClass { get; set; } = string.Empty;

        public string CategoryUrl => Category.ToString().ToLower();
    }

    public class HelpSearchViewModel
    {
        [Display(Name = "Từ khóa tìm kiếm")]
        public string Query { get; set; } = string.Empty;

        public List<HelpArticleListViewModel> Results { get; set; } = new List<HelpArticleListViewModel>();
        public int TotalCount { get; set; }
        public bool HasResults { get; set; }
    }

    public class HelpFAQViewModel
    {
        public List<HelpArticleListViewModel> FAQs { get; set; } = new List<HelpArticleListViewModel>();
    }

    public class ContactViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Chủ đề là bắt buộc")]
        [Display(Name = "Chủ đề")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự")]
        [Display(Name = "Nội dung tin nhắn")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Loại yêu cầu")]
        public ContactType Type { get; set; } = ContactType.General;
    }

    public enum ContactType
    {
        General = 1,
        Support = 2,
        Complaint = 3,
        Suggestion = 4,
        Partnership = 5
    }
}
