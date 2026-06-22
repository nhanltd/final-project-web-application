using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Order Status is required.")]
        [RegularExpression("^(Pending|Processing|Completed|Cancelled)$", ErrorMessage = "Invalid Status. Must be: Pending, Processing, Completed, Cancelled.")]
        public string Status { get; set; } = string.Empty;
    }
}
