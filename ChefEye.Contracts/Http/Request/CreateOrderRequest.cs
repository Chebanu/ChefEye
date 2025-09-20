namespace ChefEye.Contracts.Http.Request;

public class CreateOrderRequest
{
    public List<OrderMenuItemRequest> OrderMenuItems { get; init; }
}

public class OrderMenuItemRequest
{
    public Guid MenuItemId { get; init; }
    public int Quantity { get; init; }
}