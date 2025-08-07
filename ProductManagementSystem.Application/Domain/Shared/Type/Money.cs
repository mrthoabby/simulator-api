using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Shared.Type;

public class Money
{
    public decimal Value { get; private set; }
    public EnumCurrency Currency { get; private set; }

    private Money(decimal value, EnumCurrency currency)
    {
        Value = value;
        Currency = currency;
        var validator = new PriceValidator();
        var validationResult = validator.Validate(this);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }
    }

    // Constructor sin parámetros para MongoDB
    public Money()
    {
    }
    public static Money Create(decimal value, EnumCurrency currency)
    {
        return new Money(value, currency);
    }

}

public class PriceValidator : AbstractValidator<Money>
{
    public PriceValidator()
    {
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.Currency).IsInEnum().WithMessage("Currency must be a valid enum value");
    }
}