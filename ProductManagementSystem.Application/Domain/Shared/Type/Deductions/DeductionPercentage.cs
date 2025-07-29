
using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Enum;
using ProductManagementSystem.Application.Domain.Shared.Type.Prices;

namespace ProductManagementSystem.Application.Domain.Shared.Type.Deductions;

public class DeductionPercentage : Deduction
{
    public decimal Value { get; init; }

    private DeductionPercentage(string conceptCode, string name, decimal value, string? description = null)
        : base(conceptCode, name, EnumDeductionType.Percentage, EnumDeductionApplication.AddToProduct, description)
    {
        Value = value;

        var validator = new DeductionPercentageValidator();
        var validationResult = validator.Validate(this);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }
    }

    public static DeductionPercentage Create(string conceptCode, string name, decimal value, string? description = null)
    {
        return new(conceptCode, name, value, description);
    }

}

public class DeductionPercentageValidator : AbstractValidator<DeductionPercentage>
{
    public DeductionPercentageValidator()
    {
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Value must be greater than 0");
    }
}