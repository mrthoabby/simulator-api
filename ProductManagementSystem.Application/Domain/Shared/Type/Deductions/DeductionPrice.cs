using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Enum;
using ProductManagementSystem.Application.Domain.Shared.Type.Prices;

namespace ProductManagementSystem.Application.Domain.Shared.Type.Deductions;

public class DeductionPrice : Deduction
{
    public Price Value { get; init; }

    private DeductionPrice(string conceptCode, string name, Price value, string? description = null)
        : base(conceptCode, name, EnumDeductionType.FixedValue, EnumDeductionApplication.AddToProduct, description)
    {
        Value = value;

        var validator = new DeductionPriceValidator();
        var validationResult = validator.Validate(this);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }
    }

    public static DeductionPrice Create(string conceptCode, string name, Price value, string? description = null)
    {
        return new(conceptCode, name, value, description);
    }

}

public class DeductionPriceValidator : AbstractValidator<DeductionPrice>
{
    public DeductionPriceValidator()
    {
        RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
    }
}