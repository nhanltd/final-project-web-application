using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Mã khách hàng là bắt buộc.")]
        public int CustomerId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng số tiền phải lớn hơn 0.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc.")]
        [RegularExpression("^(Pending|Processing|Completed|Cancelled)$", ErrorMessage = "Trạng thái không hợp lệ. Phải là: Pending, Processing, Completed, Cancelled.")]
        public string Status { get; set; } = "Pending";
    }
}
