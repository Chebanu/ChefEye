using ChefEye.Api.Extensions;
using ChefEye.Contracts.Http.Request;
using FluentValidation;

namespace ChefEye.Api.Validator;

internal class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        this.RuleForUsername(x => x.Username);
        this.RuleForPassword(x => x.Password);
        this.RuleForOptionalEmail(x => x.Email);
        this.RuleForPhoneNumber(x => x.PhoneNumber);
    }
}