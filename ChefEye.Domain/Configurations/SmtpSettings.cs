namespace ChefEye.Domain.Configurations;

public class SmtpSettings
{
    public string Host { get; init; }
    public int Port { get; init; }
    public bool EnableSsl { get; init; }
    public string Username { get; init; }
    public string From { get; init; }
}