using ChefEye.Contracts.Http.Request;
using FluentValidation;

namespace ChefEye.Api.Validators;

public class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}