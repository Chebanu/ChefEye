namespace ChefEye.Contracts.Http.Request;

public class CreateMenuItem
{
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
}