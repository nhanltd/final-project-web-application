using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Data;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderDbContext _context;

        public OrdersController(OrderDbContext context)
        {
            _context = context;
        }

        // MVC GET: / or /Orders
        [HttpGet]
        [Route("")]
        [Route("Orders")]
        [Route("Orders/Index")]
        public async Task<IActionResult> Index(string searchString, string statusFilter, bool showTop5 = false, string highlightLinqKey = "", int pageNumber = 1)
        {
            var viewModel = new OrdersViewModel
            {
                SearchString = searchString,
                StatusFilter = statusFilter,
                ShowTop5 = showTop5
            };

            // 1. Fetch Customers for the Creation Dropdown
            viewModel.Customers = await _context.Customers
                .OrderBy(c => c.Name)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email
                })
                .ToListAsync();

            // 2. Build the main Orders Query using LINQ Query Syntax and explicit Join
            var query = from o in _context.Orders
                        join c in _context.Customers on o.CustomerId equals c.Id
                        select new OrderResponseDto
                        {
                            Id = o.Id,
                            CustomerId = o.CustomerId,
                            CustomerName = c.Name,
                            OrderDate = o.OrderDate,
                            TotalAmount = o.TotalAmount,
                            Status = o.Status
                        };

            // Determine active LINQ code to display in the UI based on filter state
            if (showTop5)
            {
                query = query.OrderByDescending(o => o.TotalAmount).Take(5);

                viewModel.ActiveLinqDesc = "LINQ Query Syntax & Join: Retrieve the top 5 highest orders by TotalAmount.";
                viewModel.ActiveLinqCode = "var query = from o in _context.Orders\n" +
                                            "            join c in _context.Customers on o.CustomerId equals c.Id\n" +
                                            "            orderby o.TotalAmount descending\n" +
                                            "            select new OrderResponseDto {\n" +
                                            "                Id = o.Id, CustomerId = o.CustomerId, CustomerName = c.Name,\n" +
                                            "                OrderDate = o.OrderDate, TotalAmount = o.TotalAmount, Status = o.Status\n" +
                                            "            };\n" +
                                            "var top5Orders = await query.Take(5).ToListAsync();";
            }
            else if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter).OrderByDescending(o => o.OrderDate);

                viewModel.ActiveLinqDesc = $"LINQ Query Syntax, Join & Filter: Retrieve orders with status '{statusFilter}'.";
                viewModel.ActiveLinqCode = "var query = from o in _context.Orders\n" +
                                            "            join c in _context.Customers on o.CustomerId equals c.Id\n" +
                                            $"            where o.Status == \"{statusFilter}\"\n" +
                                            "            orderby o.OrderDate descending\n" +
                                            "            select new OrderResponseDto {\n" +
                                            "                Id = o.Id, CustomerId = o.CustomerId, CustomerName = c.Name,\n" +
                                            "                OrderDate = o.OrderDate, TotalAmount = o.TotalAmount, Status = o.Status\n" +
                                            "            };";
            }
            else if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(o => o.CustomerName.Contains(searchString)).OrderByDescending(o => o.OrderDate);

                viewModel.ActiveLinqDesc = $"LINQ Query Syntax, Join & Search: Retrieve orders where customer name contains '{searchString}'.";
                viewModel.ActiveLinqCode = "var query = from o in _context.Orders\n" +
                                            "            join c in _context.Customers on o.CustomerId equals c.Id\n" +
                                            $"            where c.Name.Contains(\"{searchString}\")\n" +
                                            "            orderby o.OrderDate descending\n" +
                                            "            select new OrderResponseDto {\n" +
                                            "                Id = o.Id, CustomerId = o.CustomerId, CustomerName = c.Name,\n" +
                                            "                OrderDate = o.OrderDate, TotalAmount = o.TotalAmount, Status = o.Status\n" +
                                            "            };";
            }
            else
            {
                query = query.OrderByDescending(o => o.OrderDate);

                viewModel.ActiveLinqDesc = "LINQ Query Syntax & Join: Retrieve all orders, joining the related Customers, sorted by date descending.";
                viewModel.ActiveLinqCode = "var query = from o in _context.Orders\n" +
                                            "            join c in _context.Customers on o.CustomerId equals c.Id\n" +
                                            "            orderby o.OrderDate descending\n" +
                                            "            select new OrderResponseDto {\n" +
                                            "                Id = o.Id, CustomerId = o.CustomerId, CustomerName = c.Name,\n" +
                                            "                OrderDate = o.OrderDate, TotalAmount = o.TotalAmount, Status = o.Status\n" +
                                            "            };";
            }

            // 3. Apply Server-Side Pagination
            int pageSize = 5;
            if (pageNumber < 1) pageNumber = 1;

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (pageNumber > totalPages) pageNumber = totalPages;

            List<OrderResponseDto> ordersList;
            if (showTop5)
            {
                ordersList = await query.ToListAsync();
                viewModel.CurrentPage = 1;
                viewModel.TotalPages = 1;
                viewModel.PageSize = 5;
            }
            else
            {
                ordersList = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                viewModel.CurrentPage = pageNumber;
                viewModel.TotalPages = totalPages;
                viewModel.PageSize = pageSize;
            }

            viewModel.Orders = ordersList;

            // 4. Calculate Global Statistics using LINQ
            
            // LINQ: Count()
            viewModel.TotalOrders = await _context.Orders.CountAsync();

            // LINQ: Where() and SumAsync()
            viewModel.TotalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            // LINQ: GroupBy() and Count()
            var stats = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            viewModel.StatusStats = stats.ToDictionary(s => s.Status, s => s.Count);

            // Populate missing statuses with 0 count
            var allStatuses = new[] { "Pending", "Processing", "Completed", "Cancelled" };
            foreach (var status in allStatuses)
            {
                if (!viewModel.StatusStats.ContainsKey(status))
                {
                    viewModel.StatusStats[status] = 0;
                }
            }

            // If the user explicitly clicked on a statistic card to view its LINQ
            if (highlightLinqKey == "revenue")
            {
                viewModel.ActiveLinqDesc = "Filter orders by 'Completed' status and calculate the total sum of their amounts.";
                viewModel.ActiveLinqCode = "var totalRevenue = await _context.Orders\n    .Where(o => o.Status == \"Completed\")\n    .SumAsync(o => o.TotalAmount);";
            }
            else if (highlightLinqKey == "statistics")
            {
                viewModel.ActiveLinqDesc = "Group orders by status and count the number of orders in each group.";
                viewModel.ActiveLinqCode = "var stats = await _context.Orders\n    .GroupBy(o => o.Status)\n    .Select(g => new {\n        Status = g.Key,\n        Count = g.Count()\n    }).ToListAsync();";
            }

            return View(viewModel);
        }

        // MVC POST: /Orders/Create
        [HttpPost]
        [Route("Orders/Create")]
        public async Task<IActionResult> Create(int customerId, decimal totalAmount, string status)
        {
            // LINQ: Any()
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
            if (!customerExists)
            {
                TempData["ErrorMessage"] = "Customer does not exist.";
                return RedirectToAction(nameof(Index));
            }

            if (totalAmount <= 0)
            {
                TempData["ErrorMessage"] = "Amount must be greater than 0.";
                return RedirectToAction(nameof(Index));
            }

            var newOrder = new Order
            {
                CustomerId = customerId,
                TotalAmount = totalAmount,
                Status = status,
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{newOrder.Id} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // MVC POST: /Orders/UpdateStatus
        [HttpPost]
        [Route("Orders/UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // LINQ: FirstOrDefault()
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{id} status updated to {status}!";
            return RedirectToAction(nameof(Index));
        }

        // MVC POST: /Orders/Delete
        [HttpPost]
        [Route("Orders/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            // LINQ: FirstOrDefault()
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{id} deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
