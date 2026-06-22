using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure One-to-Many Relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision for TotalAmount
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // Seed Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "Nguyễn Văn A", Email = "a.nguyen@example.com" },
                new Customer { Id = 2, Name = "Trần Thị B", Email = "b.tran@example.com" },
                new Customer { Id = 3, Name = "Lê Hoàng C", Email = "c.le@example.com" },
                new Customer { Id = 4, Name = "Phạm Minh D", Email = "d.pham@example.com" },
                new Customer { Id = 5, Name = "Vũ Thị E", Email = "e.vu@example.com" }
            );

            // Seed Orders
            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, CustomerId = 1, OrderDate = DateTime.UtcNow.AddDays(-10), TotalAmount = 150000, Status = "Completed" },
                new Order { Id = 2, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-8), TotalAmount = 350000, Status = "Completed" },
                new Order { Id = 3, CustomerId = 1, OrderDate = DateTime.UtcNow.AddDays(-6), TotalAmount = 1200000, Status = "Processing" },
                new Order { Id = 4, CustomerId = 3, OrderDate = DateTime.UtcNow.AddDays(-5), TotalAmount = 450000, Status = "Pending" },
                new Order { Id = 5, CustomerId = 4, OrderDate = DateTime.UtcNow.AddDays(-4), TotalAmount = 2500000, Status = "Completed" },
                new Order { Id = 6, CustomerId = 5, OrderDate = DateTime.UtcNow.AddDays(-3), TotalAmount = 90000, Status = "Cancelled" },
                new Order { Id = 7, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-2), TotalAmount = 620000, Status = "Pending" },
                new Order { Id = 8, CustomerId = 3, OrderDate = DateTime.UtcNow.AddDays(-1), TotalAmount = 1500000, Status = "Completed" },
                new Order { Id = 9, CustomerId = 4, OrderDate = DateTime.UtcNow.AddMinutes(-120), TotalAmount = 300000, Status = "Processing" },
                new Order { Id = 10, CustomerId = 1, OrderDate = DateTime.UtcNow.AddMinutes(-30), TotalAmount = 75000, Status = "Pending" }
            );
        }
    }
}
