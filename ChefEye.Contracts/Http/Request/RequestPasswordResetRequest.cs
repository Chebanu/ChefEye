namespace ChefEye.Contracts.Http.Request;

public class RequestPasswordResetRequest
{
    public required string Email { get; init; }
}