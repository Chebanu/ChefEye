using ChefEye.Contracts.Models;
using ChefEye.Contracts.Models.ConfigModels;
using ChefEye.Domain.DbContexts;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Order = ChefEye.Contracts.Models.Order;

namespace ChefEye.Domain.Commands;

public class CreateOrderCommand : IRequest<CreateOrderCommandResult>
{
    public string Customer { get; init; }
    public List<OrderMenuItemDto> OrderMenuItemsDto { get; init; }
}

public class OrderMenuItemDto
{
    public Guid MenuItemId { get; init; }
    public int Quantity { get; init; }
}

public class CreateOrderCommandResult
{
    public bool Success { get; init; }
    public Guid OrderId { get; init; }
    public string[] Errors { get; init; }
}

public class CreateOrderCommandHandler : BaseRequestHandler<CreateOrderCommand, CreateOrderCommandResult>
{
    private readonly ChefEyeDbContext _dbContext;
    private readonly IConnectionMultiplexer _redis;
    private readonly OrderLimitsOptions _limits;

    public CreateOrderCommandHandler(
        ChefEyeDbContext dbContext,
        IConnectionMultiplexer redis,
        ILogger<BaseRequestHandler<CreateOrderCommand, CreateOrderCommandResult>> logger,
        IOptions<OrderLimitsOptions> limitsOptions) : base(logger)
    {
        _dbContext = dbContext;
        _redis = redis;
        _limits = limitsOptions.Value;
    }

    protected override async Task<CreateOrderCommandResult> HandleInternal(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new CreateOrderCommandResult
            {
                Success = false,
                Errors = ["Request is null"]
            };
        }

        var db = _redis.GetDatabase();
        var now = DateTime.UtcNow;
        bool isNight = now.Hour >= 22 || now.Hour < 6;
        int limit = isNight ? _limits.NightLimit : _limits.DayLimit;
        var currentKey = isNight ? "orders:night" : "orders:day";

        var newValue = await db.StringIncrementAsync(currentKey);

        if (newValue > limit)
        {
            await db.StringDecrementAsync(currentKey);
            return new CreateOrderCommandResult
            {
                Success = false,
                Errors = [$"Out of limit, reached maximum {limit}."]
            };
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var menuItemIds = request.OrderMenuItemsDto.Select(x => x.MenuItemId).ToList();
            var menuItems = await _dbContext.MenuItems
                .Where(mi => menuItemIds.Contains(mi.Id))
                .ToDictionaryAsync(mi => mi.Id, mi => mi.Price, cancellationToken);

            var orderId = Guid.NewGuid();

            var order = new Order
            {
                Id = orderId,
                CreatedAt = now,
                Username = request.Customer,
                OrderMenuItems = new List<OrderMenuItem>(),
                TotalAmount = request.OrderMenuItemsDto.Sum(omi => menuItems[omi.MenuItemId] * omi.Quantity),
                Status = OrderStatus.Created
            };

            var orderMenuItems = request.OrderMenuItemsDto.Select(omi => new OrderMenuItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                MenuItemId = omi.MenuItemId,
                Quantity = omi.Quantity
            }).ToList();

            order.OrderMenuItems = orderMenuItems;

            _dbContext.Orders.Add(order);
            _dbContext.OrderMenuItems.AddRange(orderMenuItems);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new CreateOrderCommandResult
            {
                Success = true,
                OrderId = orderId
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            await db.StringDecrementAsync(currentKey);

            return new CreateOrderCommandResult
            {
                Success = false,
                Errors = ["Failed to create order"]
            };
        }
    }
}