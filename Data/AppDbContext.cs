using Microsoft.EntityFrameworkCore;
using SolarStore.Models;

namespace SolarStore.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();

        b.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        b.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(i => i.OrderId);

        b.Entity<OrderItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(i => i.ProductId);

        b.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("numeric(18,2)");

        b.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("numeric(18,2)");

        b.Entity<OrderItem>()
            .Property(i => i.UnitPrice)
            .HasColumnType("numeric(18,2)");
    }
}
