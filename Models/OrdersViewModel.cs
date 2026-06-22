using System.Collections.Generic;
using OrderManagementSystem.DTOs;

namespace OrderManagementSystem.Models
{
    public class OrdersViewModel
    {
        // Data lists
        public List<OrderResponseDto> Orders { get; set; } = new List<OrderResponseDto>();
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();

        // Statistics
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public Dictionary<string, int> StatusStats { get; set; } = new Dictionary<string, int>();

        // Keep filter states
        public string SearchString { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public bool ShowTop5 { get; set; }

        // Active LINQ Query description and code for report/demo
        public string ActiveLinqDesc { get; set; } = string.Empty;
        public string ActiveLinqCode { get; set; } = string.Empty;
    }
}
