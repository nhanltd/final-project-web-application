using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Data;
using OrderManagementSystem.DTOs;

namespace OrderManagementSystem.Controllers
{
    public class CustomersController : Controller
    {
        private readonly OrderDbContext _context;

        public CustomersController(OrderDbContext context)
        {
            _context = context;
        }

        // GET: api/customers
        [HttpGet]
        [Route("api/customers")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            // LINQ: Select customers and map to CustomerDto
            var customers = await _context.Customers
                .OrderBy(c => c.Name)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email
                })
                .ToListAsync();

            return Ok(customers);
        }
    }
}
