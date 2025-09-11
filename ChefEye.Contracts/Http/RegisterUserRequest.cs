namespace ChefEye.Contracts.Http;

public class RegisterUserRequest
{
    public string Username { get; init; }
    public string Password { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
}

public class RegisterUserResponse
{
    public string UserId { get; init; }
}