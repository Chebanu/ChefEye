using ChefEye.Domain.Configurations;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ChefEye.Domain.Services;

public interface IEmailSender
{
    Task<(bool Success, string[] Errors)> SendAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml,
        CancellationToken cancellationToken);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly IOptionsMonitor<SmtpSettings> _smtp;

    public SmtpEmailSender(IOptionsMonitor<SmtpSettings> smtp)
    {
        _smtp = smtp;
    }

    public async Task<(bool Success, string[] Errors)> SendAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml,
        CancellationToken cancellationToken)
    {
        var smtpSettings = _smtp.CurrentValue;
        var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        if (string.IsNullOrWhiteSpace(smtpSettings.Host) ||
            string.IsNullOrWhiteSpace(smtpSettings.Username) ||
            string.IsNullOrWhiteSpace(smtpPassword) ||
            string.IsNullOrWhiteSpace(smtpSettings.From))
        {
            return (false, new[] { "Email service is not properly configured" });
        }

        using var client = new SmtpClient
        {
            Host = smtpSettings.Host,
            Port = smtpSettings.Port,
            EnableSsl = smtpSettings.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpSettings.Username, smtpPassword)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpSettings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage, cancellationToken);

        return (true, Array.Empty<string>());
    }
}
