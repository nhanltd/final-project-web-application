using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Total Amount must be greater than 0.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Order Status is required.")]
        [RegularExpression("^(Pending|Processing|Completed|Cancelled)$", ErrorMessage = "Invalid Status. Must be: Pending, Processing, Completed, Cancelled.")]
        public string Status { get; set; } = "Pending";
    }
}
