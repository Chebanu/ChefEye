using ChefEye.Contracts.Models;

namespace ChefEye.Contracts.Http.Response;

public class OrderResponse
{
    public Guid Id { get; init; }
    public List<OrderPosition> OrderPositions { get; init; }
    public string Customer { get; init; }
    public decimal Receipt { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class OrderPosition
{
    public Guid MenuItemId { get; init; }
    public string Name { get; init; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}