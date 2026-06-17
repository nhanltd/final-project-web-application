using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc.")]
        [RegularExpression("^(Pending|Processing|Completed|Cancelled)$", ErrorMessage = "Trạng thái không hợp lệ. Phải là: Pending, Processing, Completed, Cancelled.")]
        public string Status { get; set; } = string.Empty;
    }
}
