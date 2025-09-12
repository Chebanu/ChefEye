using ChefEye.Domain.Commands;
using ChefEye.Domain.Configurations;
using ChefEye.Domain.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChefEye.Domain;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services,
        string connectionString,
        IConfiguration jwt, IConfiguration smpt)
    {
        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<ChefEyeDbContext>()
            .AddDefaultTokenProviders();

        return services
        .AddOptions()
            .Configure<JwtSettings>(jwt)
            .Configure<SmtpSettings>(smpt)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>())
            .AddDbContext<ChefEyeDbContext>(options => options.UseSqlServer(connectionString));
    }
}
