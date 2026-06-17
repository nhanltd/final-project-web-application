using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Data;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Controllers
{
    // For MVC, we remove [ApiController] from class level or keep it if we want API behaviors.
    // If we keep [ApiController], returning View() will throw an exception in some cases because it thinks it should return JSON (or we can just keep it and add [HttpGet] / [Route] for Views).
    // Actually, it's safer to remove [ApiController] and [Route("api/[controller]")] from the class level,
    // and instead specify routes on each action method individually.
    // This allows serving the HTML View on "/" and APIs on "/api/orders".
    public class OrdersController : Controller
    {
        private readonly OrderDbContext _context;

        public OrdersController(OrderDbContext context)
        {
            _context = context;
        }

        // MVC View: GET / or GET /Orders
        [HttpGet]
        [Route("")]
        [Route("Orders")]
        [Route("Orders/Index")]
        public IActionResult Index()
        {
            return View();
        }

        // 1. GET /api/orders (Danh sách đơn hàng)
        [HttpGet]
        [Route("api/orders")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET /api/orders/{id}
        [HttpGet]
        [Route("api/orders/{id:int}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { Message = $"Không tìm thấy đơn hàng với mã {id}." });
            }

            var response = new OrderResponseDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer != null ? order.Customer.Name : "Unknown",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status
            };

            return Ok(response);
        }

        // 2. POST /api/orders (Tạo đơn hàng)
        [HttpPost]
        [Route("api/orders")]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
            {
                return BadRequest(new { Message = $"Khách hàng với Id {dto.CustomerId} không tồn tại." });
            }

            var newOrder = new Order
            {
                CustomerId = dto.CustomerId,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status,
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == newOrder.Id);

            var response = new OrderResponseDto
            {
                Id = createdOrder!.Id,
                CustomerId = createdOrder.CustomerId,
                CustomerName = createdOrder.Customer != null ? createdOrder.Customer.Name : "Unknown",
                OrderDate = createdOrder.OrderDate,
                TotalAmount = createdOrder.TotalAmount,
                Status = createdOrder.Status
            };

            return CreatedAtAction(nameof(GetOrderById), new { id = response.Id }, response);
        }

        // 3. PUT /api/orders/{id} (Cập nhật trạng thái)
        [HttpPut]
        [Route("api/orders/{id:int}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound(new { Message = $"Không tìm thấy đơn hàng với mã {id}." });
            }

            order.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật trạng thái đơn hàng thành công.", Status = order.Status });
        }

        // 4. DELETE /api/orders/{id} (Xóa đơn hàng)
        [HttpDelete]
        [Route("api/orders/{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound(new { Message = $"Không tìm thấy đơn hàng với mã {id}." });
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Đã xóa đơn hàng với mã {id} thành công." });
        }

        // 5. GET /api/orders/search (Tìm kiếm đơn hàng theo khách hàng)
        [HttpGet]
        [Route("api/orders/search")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> SearchByCustomerName([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetOrders();
            }

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Customer != null && o.Customer.Name.Contains(keyword))
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            return Ok(orders);
        }

        // 6. GET /api/orders/status/{status} (Lọc theo trạng thái)
        [HttpGet]
        [Route("api/orders/status/{status}")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> FilterByStatus(string status)
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            return Ok(orders);
        }

        // 7. GET /api/orders/top (Top 5 đơn hàng giá trị cao nhất)
        [HttpGet]
        [Route("api/orders/top")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetTopOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.TotalAmount)
                .Take(5)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            return Ok(orders);
        }

        // 8. GET /api/orders/statistics (Thống kê số lượng đơn theo trạng thái)
        [HttpGet]
        [Route("api/orders/statistics")]
        public async Task<IActionResult> GetStatusStatistics()
        {
            var stats = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(stats);
        }

        // 9. GET /api/orders/revenue (Tổng doanh thu của đơn hàng Completed)
        [HttpGet]
        [Route("api/orders/revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            return Ok(new { TotalRevenue = totalRevenue });
        }
    }
}
