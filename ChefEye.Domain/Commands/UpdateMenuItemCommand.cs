using ChefEye.Domain.DbContexts;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChefEye.Domain.Commands;

public class UpdateMenuItemCommand : IRequest<UpdateMenuItemResult>
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
}

public class UpdateMenuItemResult
{
    public bool Success { get; init; }
    public string[] Errors { get; init; }
}

public class UpdateMenuItemCommandHandler : BaseRequestHandler<UpdateMenuItemCommand, UpdateMenuItemResult>
{
    private readonly ChefEyeDbContext _context;

    public UpdateMenuItemCommandHandler(ILogger<BaseRequestHandler<UpdateMenuItemCommand, UpdateMenuItemResult>> logger, ChefEyeDbContext context) : base(logger)
    {
        _context = context;
    }

    protected override async Task<UpdateMenuItemResult> HandleInternal(UpdateMenuItemCommand request, CancellationToken cancellationToken = default)
    {
        if(request is null)
        {
            return new UpdateMenuItemResult
            {
                Success = false,
                Errors = new[] { "Request is null" }
            };
        }

        var menuItem = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (menuItem is null)
        {
            return new UpdateMenuItemResult
            {
                Success = false,
                Errors = new[] { "Menu item not found" }
            };
        }

        menuItem.Name = request.Name;
        menuItem.Description = request.Description;
        menuItem.Price = request.Price;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateMenuItemResult
        {
            Success = true
        };
    }
}