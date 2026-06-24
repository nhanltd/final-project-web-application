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
                new Customer { Id = 1, Name = "Nguyễn Văn An", Email = "an.nguyen@example.com" },
                new Customer { Id = 2, Name = "Trần Thị Bình", Email = "binh.tran@example.com" },
                new Customer { Id = 3, Name = "Lê Hoàng Cường", Email = "cuong.le@example.com" },
                new Customer { Id = 4, Name = "Phạm Minh Dũng", Email = "dung.pham@example.com" },
                new Customer { Id = 5, Name = "Vũ Thị Em", Email = "em.vu@example.com" },
                new Customer { Id = 6, Name = "Hoàng Văn Giang", Email = "giang.hoang@example.com" },
                new Customer { Id = 7, Name = "Bùi Thị Hương", Email = "huong.bui@example.com" },
                new Customer { Id = 8, Name = "Võ Văn Hải", Email = "hai.vo@example.com" },
                new Customer { Id = 9, Name = "Đặng Thị Khánh", Email = "khanh.dang@example.com" },
                new Customer { Id = 10, Name = "Đỗ Minh Long", Email = "long.do@example.com" },
                new Customer { Id = 11, Name = "Ngô Thị Mai", Email = "mai.ngo@example.com" },
                new Customer { Id = 12, Name = "Phan Văn Nam", Email = "nam.phan@example.com" },
                new Customer { Id = 13, Name = "Nguyễn Thị Quỳnh", Email = "quynh.nguyen@example.com" },
                new Customer { Id = 14, Name = "Trần Văn Sơn", Email = "son.tran@example.com" },
                new Customer { Id = 15, Name = "Lê Thị Tú", Email = "tu.le@example.com" }
            );

            // Seed Orders
            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, CustomerId = 1, OrderDate = DateTime.UtcNow.AddDays(-15), TotalAmount = 150000, Status = "Completed" },
                new Order { Id = 2, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-14), TotalAmount = 350000, Status = "Completed" },
                new Order { Id = 3, CustomerId = 3, OrderDate = DateTime.UtcNow.AddDays(-13), TotalAmount = 1200000, Status = "Processing" },
                new Order { Id = 4, CustomerId = 4, OrderDate = DateTime.UtcNow.AddDays(-12), TotalAmount = 450000, Status = "Pending" },
                new Order { Id = 5, CustomerId = 5, OrderDate = DateTime.UtcNow.AddDays(-10), TotalAmount = 2500000, Status = "Completed" },
                new Order { Id = 6, CustomerId = 6, OrderDate = DateTime.UtcNow.AddDays(-9), TotalAmount = 90000, Status = "Cancelled" },
                new Order { Id = 7, CustomerId = 7, OrderDate = DateTime.UtcNow.AddDays(-8), TotalAmount = 620000, Status = "Pending" },
                new Order { Id = 8, CustomerId = 8, OrderDate = DateTime.UtcNow.AddDays(-7), TotalAmount = 1500000, Status = "Completed" },
                new Order { Id = 9, CustomerId = 9, OrderDate = DateTime.UtcNow.AddDays(-6), TotalAmount = 300000, Status = "Processing" },
                new Order { Id = 10, CustomerId = 10, OrderDate = DateTime.UtcNow.AddDays(-5), TotalAmount = 75000, Status = "Pending" },
                new Order { Id = 11, CustomerId = 11, OrderDate = DateTime.UtcNow.AddDays(-4), TotalAmount = 850000, Status = "Completed" },
                new Order { Id = 12, CustomerId = 12, OrderDate = DateTime.UtcNow.AddDays(-3), TotalAmount = 1100000, Status = "Processing" },
                new Order { Id = 13, CustomerId = 13, OrderDate = DateTime.UtcNow.AddDays(-2), TotalAmount = 540000, Status = "Pending" },
                new Order { Id = 14, CustomerId = 14, OrderDate = DateTime.UtcNow.AddDays(-1), TotalAmount = 1950000, Status = "Completed" },
                new Order { Id = 15, CustomerId = 15, OrderDate = DateTime.UtcNow.AddMinutes(-30), TotalAmount = 120000, Status = "Cancelled" }
            );
        }
    }
}
