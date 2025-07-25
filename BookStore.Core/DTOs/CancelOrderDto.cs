using System.ComponentModel.DataAnnotations;

namespace BookStore.Core.DTOs
{
    public class CancelOrderDto
    {
        [Required(ErrorMessage = "Lý do hủy đơn hàng là bắt buộc")]
        [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự")]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
