using ChefEye.Contracts.Models;
using ChefEye.Contracts.Models.ConfigModels;
using ChefEye.Domain.DbContexts;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ChefEye.Domain.Commands;

public class CreateOrderCommand : IRequest<CreateOrderCommandResult>
{
    public string Name { get; set; }
    public List<OrderMenuItem> OrderMenuItems { get; set; } = new();
}

public enum CreateOrderCommandResultType
{
    Success,
    Failed,
}

public class CreateOrderCommandResult
{
    public CreateOrderCommandResultType CreateOrderCommandResultType { get; init; }
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
                CreateOrderCommandResultType = CreateOrderCommandResultType.Failed,
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
                CreateOrderCommandResultType = CreateOrderCommandResultType.Failed,
                Errors = [$"Out of limit, reached maximum {limit}."]
            };
        }

        try
        {
            var order = new Contracts.Models.Order
            {
                Id = Guid.NewGuid(),
                CreatedAt = now,
                CustomerId = Guid.Empty,
                OrderMenuItems = request.OrderMenuItems,
                TotalAmount = request.OrderMenuItems.Sum(omi => omi.MenuItem.Price * omi.Quantity),
                Status = OrderStatus.Created
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateOrderCommandResult
            {
                CreateOrderCommandResultType = CreateOrderCommandResultType.Success,
                OrderId = order.Id
            };
        }
        catch
        {
            await db.StringDecrementAsync(currentKey);
            throw;
        }
    }
}