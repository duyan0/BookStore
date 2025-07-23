using System.ComponentModel.DataAnnotations;

namespace BookStore.Web.Models
{
    public class AuthorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tác giả là bắt buộc")]
        [Display(Name = "Họ")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Tên đầy đủ")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Tiểu sử")]
        public string? Biography { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Số lượng sách")]
        public int BookCount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }
} 