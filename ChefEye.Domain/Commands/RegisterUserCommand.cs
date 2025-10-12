using ChefEye.Contracts.Models;
using ChefEye.Domain.Constants;
using ChefEye.Domain.DbContexts;
using ChefEye.Domain.Handlers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ChefEye.Domain.Commands;

public class RegisterUserCommand : IRequest<RegisterUserCommandResult>
{
    public required string Username { get; init; }
    public required string FullName { get; init; }
    public required string Password { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}

public class RegisterUserCommandResult
{
    public string UserId { get; init; }
    public bool Success { get; init; }
    public IEnumerable<IdentityError> Errors { get; init; }
}

internal class RegisterUserCommandHandler : BaseRequestHandler<RegisterUserCommand, RegisterUserCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ChefEyeDbContext _context;

    public RegisterUserCommandHandler(UserManager<IdentityUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        ILogger<RegisterUserCommandHandler> logger,
                                        ChefEyeDbContext context) : base(logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    protected override async Task<RegisterUserCommandResult> HandleInternal(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var isDuplicateUserByUsername = await _userManager.FindByNameAsync(request.Username);
        var isDuplicateUserByEmail = await _userManager.FindByEmailAsync(request.Email);

        if (isDuplicateUserByUsername != null || isDuplicateUserByEmail != null)
        {
            return new RegisterUserCommandResult
            {
                Success = false,
                Errors = new[] { new IdentityError { Description = "Username or Email already exists." } }
            };
        }

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new RegisterUserCommandResult
            {
                Success = false,
                Errors = result.Errors
            };
        }

        if (!await _roleManager.RoleExistsAsync(Roles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(Roles.User));
        }

        result = await _userManager.AddToRoleAsync(user, Roles.User);

        var customer = new Customer
        {
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterUserCommandResult
        {
            UserId = user.Id,
            Success = result.Succeeded,
            Errors = result.Errors
        };

    }
}