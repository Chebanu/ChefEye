using ChefEye.Domain.Services;

namespace ChefEye.Api.StartupExtensions;

public static class ServiceConfiguration
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services)
    {
        return services.AddScoped<IEmailSender, SmtpEmailSender>();
    }
}
