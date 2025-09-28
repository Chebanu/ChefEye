using ChefEye.Contracts.Models;
using ChefEye.Domain.DbContexts;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ChefEye.Domain.Commands;

public class CancelOrderCommand : IRequest<CancelOrderResult>
{
    public Guid OrderId { get; init; }
    public string User { get; init; }
}

public class CancelOrderResult
{
    public bool Success { get; init; }
    public string[] Error { get; init; }
}

public class CancelOrderCommandHandler : BaseRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    public readonly ChefEyeDbContext _context;
    private readonly IConnectionMultiplexer _redis;
    public CancelOrderCommandHandler(ILogger<BaseRequestHandler<CancelOrderCommand, CancelOrderResult>> logger,
                                    ChefEyeDbContext context,
                                    IConnectionMultiplexer redis) : base(logger)
    {
        _context = context;
        _redis = redis;
    }

    protected override async Task<CancelOrderResult> HandleInternal(CancelOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == request.OrderId && x.Username == request.User, cancellationToken);

        if (order is null)
        {
            return new CancelOrderResult
            {
                Success = false,
                Error = new[] { "Order not found" }
            };
        }

        if (order.Status is OrderStatus.InProgress or
            OrderStatus.WaitingDelivery or
            OrderStatus.Delivering or
            OrderStatus.Completed or
            OrderStatus.Cancelled)
        {
            return new CancelOrderResult
            {
                Success = false,
                Error = new[] { $"Cannot cancel order in status {order.Status}" }
            };
        }

        var redisDb = _redis.GetDatabase();

        var now = DateTime.UtcNow;
        bool isNight = now.Hour >= 22 || now.Hour < 6;
        var shiftDate = isNight && now.Hour < 6
            ? now.Date.AddDays(-1)
            : now.Date;

        string shiftType = isNight ? "night" : "day";
        string currentKey = $"orders:{shiftType}:{shiftDate:yyyy-MM-dd}";

        await redisDb.StringDecrementAsync(currentKey);

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return new CancelOrderResult { Success = true };
    }
}