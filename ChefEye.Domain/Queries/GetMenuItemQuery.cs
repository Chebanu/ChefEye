using MediatR;
using ChefEye.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using ChefEye.Domain.DbContexts;

namespace ChefEye.Domain.Queries;

public class GetMenuItemQuery : IRequest<GetMenuItemQueryResult>
{
    public Guid Id { get; init; }
}

public class GetMenuItemQueryResult
{
    public MenuItem MenuItem { get; init; }
    public bool Success { get; init; }
    public string[] Errors { get; init; } = [];
}

public class GetMenuItemQueryHandler : IRequestHandler<GetMenuItemQuery, GetMenuItemQueryResult>
{
    private readonly ChefEyeDbContext _dbContext;

    public GetMenuItemQueryHandler(ChefEyeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetMenuItemQueryResult> Handle(GetMenuItemQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (request.Id == Guid.Empty)
        {
            errors.Add("MenuItem Id is empty.");
            return new GetMenuItemQueryResult { Success = false, Errors = errors.ToArray() };
        }

        var menuItem = await _dbContext.MenuItems
            .FirstOrDefaultAsync(mi => mi.Id == request.Id, cancellationToken);

        if (menuItem == null)
        {
            errors.Add("MenuItem not found.");
            return new GetMenuItemQueryResult { Success = false, Errors = errors.ToArray() };
        }

        return new GetMenuItemQueryResult
        {
            Success = true,
            MenuItem = menuItem
        };
    }
}