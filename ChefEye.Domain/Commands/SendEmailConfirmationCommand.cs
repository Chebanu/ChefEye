using ChefEye.Domain.Handlers;
using ChefEye.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ChefEye.Domain.Commands;

public class SendEmailConfirmationCommand : IRequest<SendEmailConfirmationCommandResult>
{
    public required string Username { get; init; }
    public required string Subject { get; init; }
    public string HtmlMessage { get; init; }
    public required string BaseUrl { get; init; }
}

public class SendEmailConfirmationCommandResult
{
    public bool Success { get; init; }
    public string[] Errors { get; init; } = Array.Empty<string>();
}

public class SendEmailConfirmationCommandHandler
    : BaseRequestHandler<SendEmailConfirmationCommand, SendEmailConfirmationCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;

    public SendEmailConfirmationCommandHandler(UserManager<IdentityUser> userManager,
                                              IEmailSender emailSender,
                                              ILogger<BaseRequestHandler<SendEmailConfirmationCommand,
                                                        SendEmailConfirmationCommandResult>> logger) : base(logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    protected override async Task<SendEmailConfirmationCommandResult> HandleInternal(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return new SendEmailConfirmationCommandResult
            {
                Success = false,
                Errors = new[] { "User not found" }
            };
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{request.BaseUrl.TrimEnd('/')}/users/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

        var body = string.Join('\n', request.HtmlMessage, confirmationLink);
        var isBodyHtml = !string.IsNullOrWhiteSpace(request.HtmlMessage);

        var result = await _emailSender.SendAsync(user.Email!, request.Subject, body, isBodyHtml, cancellationToken);

        return new SendEmailConfirmationCommandResult
        {
            Success = result.Success,
            Errors = result.Errors
        };
    }
}