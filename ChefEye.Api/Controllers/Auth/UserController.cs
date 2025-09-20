using ChefEye.Api.Constants;
using ChefEye.Api.Extensions;
using ChefEye.Contracts.Http;
using ChefEye.Contracts.Http.Request;
using ChefEye.Contracts.Http.Response;
using ChefEye.Domain.Commands;
using ChefEye.Domain.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace ChefEye.Api.Controllers;

[Route("users")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<RegisterUserRequest> _registerUserValidator;
    private readonly IValidator<AuthenticateUserRequest> _authenticateUserValidator;
    private readonly IValidator<AdminUpdateUserRequest> _adminUpdateUserValidator;
    private readonly IValidator<ConfirmEmailRequest> _confirmEmailValidator;
    private readonly IValidator<RequestPasswordResetRequest> _requestPasswordResetValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

    public UsersController(IMediator mediator,
                          IValidator<RegisterUserRequest> registerUserValidator,
                          IValidator<AuthenticateUserRequest> authenticateUserValidator,
                          IValidator<AdminUpdateUserRequest> adminUpdateUserValidator,
                          IValidator<ConfirmEmailRequest> confirmEmailValidator,
                          IValidator<RequestPasswordResetRequest> requestPasswordResetValidator,
                          IValidator<ResetPasswordRequest> resetPasswordValidator)
    {
        _mediator = mediator;
        _registerUserValidator = registerUserValidator;
        _authenticateUserValidator = authenticateUserValidator;
        _adminUpdateUserValidator = adminUpdateUserValidator;
        _confirmEmailValidator = confirmEmailValidator;
        _requestPasswordResetValidator = requestPasswordResetValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request,
                                                    CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
            });
        }

        var command = new RegisterUserCommand
        {
            Username = request.Username,
            FullName = request.FullName,
            Password = request.Password,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = result.Errors.Select(x => x.Description).ToArray()
            });
        }

        var emailCommand = new SendEmailConfirmationCommand
        {
            Username = request.Username,
            Subject = "Confirm your email",
            BaseUrl = $"{Request.Scheme}://{Request.Host}",
            HtmlMessage = "..."
        };

        var emailResult = await _mediator.Send(emailCommand);

        if (!emailResult.Success)
        {
            return Created(@$"{request.Username} username has been created.
However, the confirmation mail failed. Reason: {result.Errors.Select(x => x.Description).ToList()}",
            new RegisterUserResponse
            {
                UserId = result.UserId
            });
        }

        return Created($"{request.Username} username has been created", new RegisterUserResponse
        {
            UserId = result.UserId
        });
    }

    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest request,
                                                CancellationToken cancellationToken = default)
    {
        var validationResult = await _confirmEmailValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = ["Confirmation link is invalid"]
            });
        }

        var query = new ConfirmEmailQuery
        {
            UserId = request.UserId,
            Token = request.Token
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result.Success)
        {
            return Ok(new
            {
                message = "Email confirmed successfully!",
                success = true
            });
        }

        return BadRequest(new ErrorResponse
        {
            Errors = result.Errors
        });
    }

    [HttpGet("resent-email")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> ConfirmEmail(CancellationToken cancellationToken = default)
    {
        var emailCommand = new SendEmailConfirmationCommand
        {
            Username = User.Identity.Name,
            Subject = "Confirm your email",
            BaseUrl = $"{Request.Scheme}://{Request.Host}",
            HtmlMessage = "..."
        };

        var emailResult = await _mediator.Send(emailCommand);

        if (!emailResult.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = emailResult.Errors
            });
        }

        return Ok($"Confirmation letter was sent");
    }

    [HttpPost("authenticate")]
    [ProducesResponseType(typeof(AuthenticateUserResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _authenticateUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage)
            });
        }

        var command = new AuthenticateUserCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);
        return !result.Success ?
            BadRequest(new ErrorResponse
            {
                Errors = result.Errors
            })
            : Ok(new AuthenticateUserResponse
            {
                Token = result.Token,
            });
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MeResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public IActionResult GetCurrentUser()
    {
        return Ok(new MeResponse
        {
            Username = User.Identity.Name,
            Roles = User.Claims.Roles().ToArray()
        });
    }

    [HttpPut("admin")]
    [Authorize(Policy = AuthorizePolicies.Admin)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> AdminUpdateUser([FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _adminUpdateUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage)
            });
        }

        var command = new AdminUpdateUserCommand
        {
            Username = request.Username,
            RoleToAdd = request.Roles.Where(x => x.Action is UpdateRoleAction.Add).Select(x => x.Role).ToArray(),
            RoleToRemove = request.Roles.Where(x => x.Action is UpdateRoleAction.Remove).Select(x => x.Role).ToArray()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return !result.Success
            ? BadRequest(new ErrorResponse
            {
                Errors = result.Errors
            })
            : Ok();
    }

    [HttpPost("request-password-reset")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _requestPasswordResetValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
            });
        }

        var command = new RequestPasswordResetCommand
        {
            Email = request.Email,
            BaseUrl = $"{Request.Scheme}://{Request.Host}"
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { message = "Reset mail just sent" });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _resetPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
            });
        }

        var command = new ResetPasswordCommand
        {
            Email = request.Email,
            Token = request.Token,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = result.Errors
            });
        }

        return Ok(new { message = "Password has been reset successfully" });
    }
}