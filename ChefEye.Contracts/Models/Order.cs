using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChefEye.Contracts.Models;

public class Order
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    [ForeignKey(nameof(Customer))]
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
    public List<OrderMenuItem> OrderMenuItems { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}

public enum OrderStatus
{
    Created,
    OrderAccepted,
    InProgress,
    WaitingDelivery,
    Delivering,
    Completed,
    Cancelled
}