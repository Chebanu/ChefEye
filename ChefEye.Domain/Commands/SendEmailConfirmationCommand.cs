using Azure.Core;
using ChefEye.Domain.Configurations;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;

namespace ChefEye.Domain.Commands;

public class SendEmailConfirmationCommand : IRequest<SendEmailConfirmationCommandResult>
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Subject { get; init; }
    public string HtmlMessage { get; init; }
    public required string BaseUrl { get; init; }  // Add this property
}

public class SendEmailConfirmationCommandResult
{
    public bool Success { get; init; }
    public string[] Errors { get; init; } = Array.Empty<string>();
}

public class SendEmailConfirmationCommandHandler : BaseRequestHandler<SendEmailConfirmationCommand, SendEmailConfirmationCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptionsMonitor<SmtpSettings> _smtp;

    public SendEmailConfirmationCommandHandler(
        IOptionsMonitor<SmtpSettings> smtpOptions,
        ILogger<BaseRequestHandler<SendEmailConfirmationCommand, SendEmailConfirmationCommandResult>> logger,
        UserManager<IdentityUser> userManager) : base(logger)
    {
        _smtp = smtpOptions;
        _userManager = userManager;
    }

    protected override async Task<SendEmailConfirmationCommandResult> HandleInternal(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
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

            var smtpSettings = _smtp.CurrentValue;
            string smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

            if (string.IsNullOrWhiteSpace(smtpSettings.Host) ||
                string.IsNullOrWhiteSpace(smtpSettings.Username) ||
                string.IsNullOrWhiteSpace(smtpPassword) ||
                string.IsNullOrWhiteSpace(smtpSettings.From))
            {
                return new SendEmailConfirmationCommandResult
                {
                    Success = false,
                    Errors = new[] { "Email service is not properly configured" }
                };
            }

            using var client = new SmtpClient();
            client.Host = smtpSettings.Host;
            client.Port = smtpSettings.Port;
            client.EnableSsl = smtpSettings.EnableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(smtpSettings.Username, smtpPassword);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.From),
                Subject = request.Subject,
                Body = confirmationLink,
                IsBodyHtml = !string.IsNullOrWhiteSpace(request.HtmlMessage)
            };

            mailMessage.To.Add(request.Email);

            await client.SendMailAsync(mailMessage, cancellationToken);

            return new SendEmailConfirmationCommandResult
            {
                Success = true
            };
        }
        catch (SmtpException smtpEx)
        {
            return new SendEmailConfirmationCommandResult
            {
                Success = false,
                Errors = new[] { $"Failed to send email due to mail server error. {smtpEx}" }
            };
        }
        catch (ArgumentException argEx)
        {
            return new SendEmailConfirmationCommandResult
            {
                Success = false,
                Errors = new[] { $"Invalid email address or configuration. {argEx}" }
            };
        }
        catch (InvalidOperationException ioEx)
        {
            return new SendEmailConfirmationCommandResult
            {
                Success = false,
                Errors = new[] { $"Email service configuration error.{ioEx}" }
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new SendEmailConfirmationCommandResult
            {
                Success = false,
                Errors = new[] { "Email sending was cancelled" }
            };
        }
        catch (Exception)
        {
            return new SendEmailConfirmationCommandResult
            {
                Success = false,
                Errors = new[] { "An unexpected error occurred while sending email" }
            };
        }
    }
}