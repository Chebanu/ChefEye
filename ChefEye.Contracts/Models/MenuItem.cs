using System.ComponentModel.DataAnnotations;

namespace ChefEye.Contracts.Models;

public class MenuItem
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}