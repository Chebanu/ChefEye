using ChefEye.Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace ChefEye.Domain.Seeds;


public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = Guid.Parse("98758B15-218D-4711-A0DD-F7A87E80197F"), Name = "Pancake", Price = 4.5m }
        );
        
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = Guid.Parse("1CEDFF26-8134-4C53-B22A-7F3E61ABB594"), Name = "Coffe", Price = 3.5m }
        );
        
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = Guid.Parse("E71FFCD6-01C2-48E9-8A11-B9E8CE2E9AEF"), Name = "Pasta", Price = 13m }
        );
    }
}