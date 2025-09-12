using ChefEye.Contracts.Http.Response;
using ChefEye.Contracts.Models;
using ChefEye.Domain.DbContexts;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChefEye.Domain.Queries;

public class GetOrdersQuery : IRequest<GetOrdersQueryResult>
{
    public Guid? Id { get; init; }
}

public class GetOrdersQueryResult
{
    public List<OrderResponse> OrderResponses { get; init; } = new();
    public bool Success { get; init; }
    public string[] Errors { get; init; } = [];
}

public class GetOrdersQueryHandler : BaseRequestHandler<GetOrdersQuery, GetOrdersQueryResult>
{
    private readonly ChefEyeDbContext _dbContext;

    public GetOrdersQueryHandler(ILogger<BaseRequestHandler<GetOrdersQuery, GetOrdersQueryResult>> logger,
        ChefEyeDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    protected override async Task<GetOrdersQueryResult> HandleInternal(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        IQueryable<Order> query = _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderMenuItems)
                .ThenInclude(omi => omi.MenuItem);

        if (request.Id.HasValue && request.Id != Guid.Empty)
        {
            query = query.Where(o => o.Id == request.Id.Value);
        }

        var orders = await query.ToListAsync(cancellationToken);

        if (!orders.Any())
        {
            errors.Add("No orders found.");
            return new GetOrdersQueryResult
            {
                Success = false,
                Errors = errors.ToArray()
            };
        }

        var responses = orders.Select(order => new OrderResponse
        {
            Id = order.Id,
            Customer = order.Customer.FullName,
            Receipt = order.TotalAmount,
            OrderPositions = order.OrderMenuItems.Select(omi => new OrderPosition
            {
                MenuItemId = omi.MenuItemId,
                Name = omi.MenuItem.Name,
                Quantity = omi.Quantity,
                Price = omi.MenuItem.Price
            }).ToList(),
            Status = order.Status,
            CreatedAt = order.CreatedAt
        }).ToList();

        return new GetOrdersQueryResult
        {
            Success = true,
            OrderResponses = responses
        };
    }
}