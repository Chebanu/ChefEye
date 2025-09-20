using ChefEye.Contracts.Http.Request;
using FluentValidation;

namespace ChefEye.Api.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        //RuleFor(x => x.CustomerId)
        //    .NotEmpty()
        //    .WithMessage("Customer ID is required.");

        RuleFor(x => x.OrderMenuItems)
            .NotNull()
            .WithMessage("Order items list must not be null.")
            .NotEmpty()
            .WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.OrderMenuItems).ChildRules(menuItem =>
        {
            menuItem.RuleFor(x => x.MenuItemId)
                .NotEmpty()
                .WithMessage("Menu item ID is required.");

            menuItem.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Quantity must not exceed 100.");
        });

        RuleFor(x => x.OrderMenuItems)
            .Must(items => items == null || items.Select(i => i.MenuItemId).Distinct().Count() == items.Count)
            .WithMessage("Order must not contain duplicate menu items.");
    }
}