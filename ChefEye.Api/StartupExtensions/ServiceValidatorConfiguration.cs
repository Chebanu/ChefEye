using ChefEye.Api.Validator;
using ChefEye.Contracts.Http;
using ChefEye.Contracts.Http.Request;
using FluentValidation;

namespace ChefEye.Api.StartupExtensions;

public static class ServiceValidatorConfiguration
{
    public static IServiceCollection AddValidatorConfiguration(this IServiceCollection services)
    {
        return services.AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>()
                        .AddScoped<IValidator<AuthenticateUserRequest>, AuthenticateUserRequestValidator>()
                        .AddScoped<IValidator<AdminUpdateUserRequest>, AdminUpdateUserRequestValidator>();
    }
}