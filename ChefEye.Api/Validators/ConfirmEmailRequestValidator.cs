using ChefEye.Contracts.Http.Request;
using FluentValidation;

namespace ChefEye.Api.Validators;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
    }
}