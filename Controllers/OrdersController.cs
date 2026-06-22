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
        public async Task<IActionResult> Index(string searchString, string statusFilter, bool showTop5 = false, string highlightLinqKey = "")
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

            // 2. Build the main Orders Query using LINQ
            IQueryable<Order> query = _context.Orders.Include(o => o.Customer);

            // Determine active LINQ code to display in the UI based on filter state
            if (showTop5)
            {
                // LINQ: OrderByDescending and Take
                query = query.OrderByDescending(o => o.TotalAmount).Take(5);

                viewModel.ActiveLinqDesc = "Sort all orders descending by TotalAmount and retrieve the first 5 records.";
                viewModel.ActiveLinqCode = "var orders = await _context.Orders\n    .Include(o => o.Customer)\n    .OrderByDescending(o => o.TotalAmount)\n    .Take(5)\n    .ToListAsync();";
            }
            else if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                // LINQ: Where
                query = query.Where(o => o.Status == statusFilter).OrderByDescending(o => o.OrderDate);

                viewModel.ActiveLinqDesc = $"Filter orders that match the specified status '{statusFilter}'.";
                viewModel.ActiveLinqCode = $"var orders = await _context.Orders\n    .Include(o => o.Customer)\n    .Where(o => o.Status == \"{statusFilter}\")\n    .OrderByDescending(o => o.OrderDate)\n    .ToListAsync();";
            }
            else if (!string.IsNullOrWhiteSpace(searchString))
            {
                // LINQ: Where and Contains
                query = query.Where(o => o.Customer != null && o.Customer.Name.Contains(searchString)).OrderByDescending(o => o.OrderDate);

                viewModel.ActiveLinqDesc = $"Search for orders where the related customer's name contains '{searchString}'.";
                viewModel.ActiveLinqCode = $"var orders = await _context.Orders\n    .Include(o => o.Customer)\n    .Where(o => o.Customer.Name.Contains(\"{searchString}\"))\n    .OrderByDescending(o => o.OrderDate)\n    .ToListAsync();";
            }
            else
            {
                // LINQ: Default list all
                query = query.OrderByDescending(o => o.OrderDate);

                viewModel.ActiveLinqDesc = "Retrieve all orders, eagerly load the related Customer details, and sort by date descending.";
                viewModel.ActiveLinqCode = "var orders = await _context.Orders\n    .Include(o => o.Customer)\n    .OrderByDescending(o => o.OrderDate)\n    .ToListAsync();";
            }

            // Execute main list query
            var ordersList = await query.ToListAsync();
            viewModel.Orders = ordersList.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status
            }).ToList();

            // 3. Calculate Global Statistics using LINQ
            
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
