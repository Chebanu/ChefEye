using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ChefEye.Domain.Commands;

public class ResetPasswordCommand : IRequest<ResetPasswordCommandResult>
{
    public required string Email { get; init; }
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}

public class ResetPasswordCommandResult
{
    public bool Success { get; init; }
    public string[] Errors { get; init; } = Array.Empty<string>();
}

public class ResetPasswordCommandHandler : BaseRequestHandler<ResetPasswordCommand, ResetPasswordCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;

    public ResetPasswordCommandHandler(
        ILogger<ResetPasswordCommandHandler> logger,
        UserManager<IdentityUser> userManager) : base(logger)
    {
        _userManager = userManager;
    }

    protected override async Task<ResetPasswordCommandResult> HandleInternal(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new ResetPasswordCommandResult
            {
                Success = false,
                Errors = new[] { "Invalid reset attempt" }
            };
        }

        var decodedBytes = WebEncoders.Base64UrlDecode(request.Token);
        var decodedToken = Encoding.UTF8.GetString(decodedBytes);

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        return new ResetPasswordCommandResult
        {
            Success = result.Succeeded,
            Errors = result.Succeeded ? Array.Empty<string>() : result.Errors.Select(e => e.Description).ToArray()
        };
    }
}