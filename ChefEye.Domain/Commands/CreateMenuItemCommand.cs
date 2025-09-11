using Microsoft.Extensions.Logging;
using ChefEye.Domain.Handlers;
using MediatR;
using ChefEye.Contracts.Models;
using ChefEye.Domain.DbContexts;

namespace ChefEye.Domain.Commands;

public class CreateMenuItemCommand : IRequest<CreateMenuItemCommandResult>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
}

public class CreateMenuItemCommandResult
{
    public Guid MenuItemId { get; init; }
    public bool Sucsess { get; init; }
    public string[] Errors { get; init; }
}

public class CreateMenuItemCommandHandler : BaseRequestHandler<CreateMenuItemCommand, CreateMenuItemCommandResult>
{
    private readonly ChefEyeDbContext _dbContext;
    public CreateMenuItemCommandHandler(ILogger<BaseRequestHandler<CreateMenuItemCommand, CreateMenuItemCommandResult>> logger,
        ChefEyeDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    protected override async Task<CreateMenuItemCommandResult> HandleInternal(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new CreateMenuItemCommandResult
            {
                Sucsess = false,
                Errors = ["Request is null"]
            };
        }

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        _dbContext.MenuItems.Add(menuItem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateMenuItemCommandResult
        {
            MenuItemId = menuItem.Id,
            Sucsess = true
        };
    }
}