using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ChefEye.Domain.Queries;

public class ConfirmEmailQuery : IRequest<ConfirmEmailQueryResult>
{
    public required string UserId { get; init; }
    public required string Token { get; init; }
}

public class ConfirmEmailQueryResult
{
    public bool Success { get; init; }
    public string[] Errors { get; init; }
}

public class ConfirmEmailQueryHandler : BaseRequestHandler<ConfirmEmailQuery, ConfirmEmailQueryResult>
{
    private readonly UserManager<IdentityUser> _userManager;

    public ConfirmEmailQueryHandler(ILogger<BaseRequestHandler<ConfirmEmailQuery, ConfirmEmailQueryResult>> logger,
        UserManager<IdentityUser> userManager) : base(logger)
    {
        _userManager = userManager;
    }

    protected override async Task<ConfirmEmailQueryResult> HandleInternal(ConfirmEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
        {
            return new ConfirmEmailQueryResult
            {
                Success = false,
                Errors = new[] { "User not found" }
            };
        }
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        if (result.Succeeded)
        {
            return new ConfirmEmailQueryResult
            {
                Success = true
            };
        }
        else
        {
            return new ConfirmEmailQueryResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description).ToArray()
            };
        }
    }
}