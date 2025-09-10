using Microsoft.EntityFrameworkCore;
using ChefEye.Contracts.Models;

namespace ChefEye.Domain;

public class ChefEyeDbContext : DbContext
{

    public DbSet<Order> Orders { set; get; }
    public DbSet<Customer> Customers { set; get; }
    public DbSet<MenuItem> MenuItems { set; get; }
    public DbSet<OrderMenuItem> OrderMenuItems { set; get; }

    public ChefEyeDbContext(DbContextOptions<ChefEyeDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        base.OnModelCreating(modelBuilder);
    }

}