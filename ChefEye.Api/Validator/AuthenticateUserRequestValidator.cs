using ChefEye.Api.Extensions;
using ChefEye.Contracts.Http.Request;
using FluentValidation;

namespace ChefEye.Api.Validator;

internal class AuthenticateUserRequestValidator : AbstractValidator<AuthenticateUserRequest>
{
    public AuthenticateUserRequestValidator()
    {
        this.RuleForUsername(x => x.Username);
        this.RuleForPassword(x => x.Password);
    }
}