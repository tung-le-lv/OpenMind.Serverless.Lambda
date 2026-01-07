using FluentValidation;
using Order.Application.Commands;

namespace Order.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.")
            .MaximumLength(100).WithMessage("Customer ID must not exceed 100 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one order item is required.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());

        When(x => x.ShippingAddress != null, () =>
        {
            RuleFor(x => x.ShippingAddress!).SetValidator(new AddressDtoValidator());
        });
    }
}

public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
    }
}

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200).WithMessage("Street must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");
    }
}
