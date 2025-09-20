namespace ChefEye.Contracts.Http.Request;

public class ResetPasswordRequest
{
    public required string Email { get; init; }
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}