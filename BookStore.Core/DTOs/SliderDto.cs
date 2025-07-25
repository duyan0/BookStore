using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class SliderDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string ButtonText { get; set; } = string.Empty;
        public string ButtonStyle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateSliderDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
        [StringLength(255, ErrorMessage = "URL hình ảnh không được vượt quá 255 ký tự")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "URL liên kết không được vượt quá 255 ký tự")]
        public string LinkUrl { get; set; } = string.Empty;

        [Range(0, 999, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 999")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [StringLength(100, ErrorMessage = "Text button không được vượt quá 100 ký tự")]
        public string ButtonText { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Style button không được vượt quá 50 ký tự")]
        public string ButtonStyle { get; set; } = "primary";
    }

    public class UpdateSliderDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
        [StringLength(255, ErrorMessage = "URL hình ảnh không được vượt quá 255 ký tự")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "URL liên kết không được vượt quá 255 ký tự")]
        public string LinkUrl { get; set; } = string.Empty;

        [Range(0, 999, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 999")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [StringLength(100, ErrorMessage = "Text button không được vượt quá 100 ký tự")]
        public string ButtonText { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Style button không được vượt quá 50 ký tự")]
        public string ButtonStyle { get; set; } = "primary";
    }
}
