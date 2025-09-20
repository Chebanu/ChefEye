namespace ChefEye.Contracts.Http.Request;

public  class CreateMenuItemRequest
{
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
}