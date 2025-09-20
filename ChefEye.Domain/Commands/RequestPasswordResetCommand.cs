using ChefEye.Domain.Handlers;
using ChefEye.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ChefEye.Domain.Commands;

public class RequestPasswordResetCommand : IRequest<RequestPasswordResetCommandResult>
{
    public required string Email { get; init; }
    public required string BaseUrl { get; init; }
}

public class RequestPasswordResetCommandResult
{
    public bool Success { get; init; }
    public string[] Errors { get; init; } = Array.Empty<string>();
}

public class RequestPasswordResetCommandHandler : BaseRequestHandler<RequestPasswordResetCommand, RequestPasswordResetCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;

    public RequestPasswordResetCommandHandler(
        ILogger<RequestPasswordResetCommandHandler> logger,
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender) : base(logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    protected override async Task<RequestPasswordResetCommandResult> HandleInternal(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new RequestPasswordResetCommandResult { Success = true };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Кодируем токен
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

        var resetLink = $"{request.BaseUrl}/reset-password?email={Uri.EscapeDataString(request.Email)}&token={encodedToken}";

        string subject = "Password Reset Request";
        string htmlMessage = $@"
                <h2>Password Reset Request</h2>
                <p>To reset your password, click the link below:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>";

        var result = await _emailSender.SendAsync(user.Email, subject, htmlMessage, !string.IsNullOrWhiteSpace(htmlMessage), cancellationToken);

        return new RequestPasswordResetCommandResult
        {
            Success = result.Success,
            Errors = result.Success ? Array.Empty<string>() : result.Errors
        };
    }
}