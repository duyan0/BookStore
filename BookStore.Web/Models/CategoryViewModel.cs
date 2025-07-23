using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Số lượng sách")]
        public int BookCount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }
} 