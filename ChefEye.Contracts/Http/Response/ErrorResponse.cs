namespace ChefEye.Contracts.Http.Response;

public class ErrorResponse
{
    public IEnumerable<string> Errors { get; init; }
}