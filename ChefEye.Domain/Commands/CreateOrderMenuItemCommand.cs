using ChefEye.Domain.DbContexts;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChefEye.Domain.Commands;

public class CreateOrderMenuItemCommand : IRequest<CreateOrderMenuItemCommandResult>
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderMenuItemCommandResult
{
    public bool Sucsess { get; init; }
    public string[] Errors { get; init; }
}

public class CreateOrderMenuItemCommandHandler : BaseRequestHandler<CreateOrderMenuItemCommand, CreateOrderMenuItemCommandResult>
{
    private readonly ChefEyeDbContext _dbContext;
    public CreateOrderMenuItemCommandHandler(
        ILogger<BaseRequestHandler<CreateOrderMenuItemCommand, CreateOrderMenuItemCommandResult>> logger,
        ChefEyeDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    protected override async Task<CreateOrderMenuItemCommandResult> HandleInternal(CreateOrderMenuItemCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (request is null)
        {
            errors.Add("Request is null");
            return new CreateOrderMenuItemCommandResult { Sucsess = false, Errors = errors.ToArray() };
        }

        if (request.Quantity <= 0)
        {
            errors.Add("Quantity must be greater than zero");
        }

        var order = await _dbContext.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
        {
            errors.Add("Order not found");
        }

        var menuItem = await _dbContext.MenuItems.FindAsync(new object[] { request.MenuItemId }, cancellationToken);
        if (menuItem is null)
        {
            errors.Add("Menu item not found");
        }

        if (errors.Count > 0)
        {
            return new CreateOrderMenuItemCommandResult { Sucsess = false, Errors = errors.ToArray() };
        }

        var orderMenuItem = new Contracts.Models.OrderMenuItem
        {
            Id = Guid.NewGuid(),
            MenuItemId = request.MenuItemId,
            OrderId = request.OrderId,
            Quantity = request.Quantity
        };

        _dbContext.OrderMenuItems.Add(orderMenuItem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderMenuItemCommandResult { Sucsess = true, Errors = [] };
    }
}