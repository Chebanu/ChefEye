using StackExchange.Redis;
using ChefEye.Domain.DbContexts;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChefEye.Domain.Services;

public class OrderLimitInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabase _redis;

    public OrderLimitInitializer(IServiceProvider serviceProvider, IConnectionMultiplexer redis)
    {
        _serviceProvider = serviceProvider;
        _redis = redis.GetDatabase();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChefEyeDbContext>();

        var now = DateTime.UtcNow;

        var today = now.Date;
        var dayOrders = await dbContext.Orders
            .CountAsync(o => o.CreatedAt >= today.AddHours(6) && o.CreatedAt < today.AddHours(22), cancellationToken);

        var nightOrders = await dbContext.Orders
            .CountAsync(o =>
                (o.CreatedAt < today.AddHours(6) && o.CreatedAt >= today)
                || o.CreatedAt >= today.AddHours(22),
                cancellationToken);

        await _redis.StringSetAsync("orders:day", dayOrders);
        await _redis.StringSetAsync("orders:night", nightOrders);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
