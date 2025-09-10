using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChefEye.Contracts.Models;

public class OrderMenuItem
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(MenuItem))]
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public int Quantity { get; set; }
}