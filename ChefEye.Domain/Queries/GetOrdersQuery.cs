using ChefEye.Contracts.Http.Response;
using ChefEye.Contracts.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersQueryResult>
{
    private readonly ChefEyeDbContext _dbContext;

    public GetOrdersQueryHandler(ChefEyeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetOrdersQueryResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
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