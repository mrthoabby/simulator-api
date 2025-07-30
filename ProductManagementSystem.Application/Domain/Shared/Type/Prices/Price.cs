using FluentValidation;

namespace ProductManagementSystem.Application.Domain.Shared.Type.Prices;

public class Price
{
    public decimal Value { get; private set; }
    public EnumCurrency Currency { get; private set; }

    private Price(decimal value, EnumCurrency currency)
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
    public static Price Create(decimal value, EnumCurrency currency)
    {
        return new Price(value, currency);
    }

}

public class PriceValidator : AbstractValidator<Price>
{
    public PriceValidator()
    {
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.Currency).IsInEnum().WithMessage("Currency must be a valid enum value");
    }
}