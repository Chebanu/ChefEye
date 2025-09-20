namespace ChefEye.Contracts.Http.Request;

public class ConfirmEmailRequest
{
    public string UserId { get; set; }
    public string Token { get; set; }
}