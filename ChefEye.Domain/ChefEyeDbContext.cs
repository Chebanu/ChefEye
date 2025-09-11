using ChefEye.Contracts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChefEye.Domain.DbContexts;

public class ChefEyeDbContext : IdentityDbContext<IdentityUser>
{

    public DbSet<Order> Orders { set; get; }
    public DbSet<Customer> Customers { set; get; }
    public DbSet<MenuItem> MenuItems { set; get; }
    public DbSet<OrderMenuItem> OrderMenuItems { set; get; }

    public ChefEyeDbContext(DbContextOptions<ChefEyeDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }

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