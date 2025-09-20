using ChefEye.Api.Constants;
using ChefEye.Api.StartupExtensions;
using ChefEye.Contracts.Models.ConfigModels;
using ChefEye.Domain;
using ChefEye.Domain.Constants;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ChefEye",
        Description = "ChefEye",
    });
});

builder.Services.AddControllers()
    .AddOData(opt =>
    {
        var odataBuilder = new ODataConventionModelBuilder();

        opt.AddRouteComponents("odata", odataBuilder.GetEdmModel())
            .Select().Filter().OrderBy().Expand();
    });
    

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddDomainServices(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    builder.Configuration.GetSection("Jwt"),
    builder.Configuration.GetSection("SmtpSettings")
);

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AuthorizePolicies.Admin, policy => policy.RequireClaim(ClaimTypes.Role, Roles.Admin))
    .AddPolicy(AuthorizePolicies.User, policy => policy.RequireClaim(ClaimTypes.Role, Roles.User));

AuthenticationValidator.AddAuthenticationService(builder.Services, builder.Configuration);

ServiceValidatorConfiguration.AddValidatorConfiguration(builder.Services);
ServiceConfiguration.AddServiceConfiguration(builder.Services);

builder.Services.Configure<OrderLimitsOptions>(
    builder.Configuration.GetSection("OrderLimits"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();