using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Shared.Type;

public class Deduction
{
    public string ConceptCode { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public EnumDeductionType Type { get; private set; }
    public decimal? Percentage { get; private set; }
    public Money? Price { get; private set; }
    public EnumDeductionApplication Application { get; private set; }

    protected Deduction(string conceptCode, string name, EnumDeductionApplication application, Money money, string? description = null)
    {
        ConceptCode = conceptCode;
        Name = name;
        Application = application;
        Description = description;
        Price = money;
        Percentage = null;
        Type = EnumDeductionType.FixedValue;
    }

    protected Deduction(string conceptCode, string name, EnumDeductionApplication application, string? description = null)
    {
        ConceptCode = conceptCode;
        Name = name;
        Application = application;
        Description = description;
    }

    protected Deduction(string conceptCode, string name, EnumDeductionApplication application, decimal percentage, string? description = null)
    {
        ConceptCode = conceptCode;
        Name = name;
        Application = application;
        Description = description;
        Percentage = percentage;
        Price = null;
        Type = EnumDeductionType.Percentage;
    }



    public static Deduction Create(string conceptCode, string name, EnumDeductionApplication application, decimal percentage, string? description = null)
    {
        var deduction = new Deduction(conceptCode, name, application, percentage, description);

        var validator = new DeductionValidator();
        var validationResult = validator.Validate(deduction);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return deduction;
    }

    public static Deduction Create(string conceptCode, string name, EnumDeductionApplication application, Money price, string? description = null)
    {
        var deduction = new Deduction(conceptCode, name, application, price, description);

        var validator = new DeductionValidator();
        var validationResult = validator.Validate(deduction);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return deduction;
    }

    public static Deduction Create(string conceptCode, string name, EnumDeductionApplication application, string? description = null)
    {
        var deduction = new Deduction(conceptCode, name, application, description);

        var validator = new DeductionValidator();
        var validationResult = validator.Validate(deduction);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return deduction;
    }
}

public class DeductionValidator : AbstractValidator<Deduction>
{
    public DeductionValidator()
    {
        RuleFor(x => x.ConceptCode)
            .NotEmpty().WithMessage("Deduction concept code is required")
            .MaximumLength(50).WithMessage("Deduction concept code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Deduction name is required")
            .MaximumLength(200).WithMessage("Deduction name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Deduction description cannot exceed 500 characters");

        RuleFor(x => x.Application)
            .IsInEnum().WithMessage("Deduction application must be a valid enum value");

        RuleFor(x => x)
            .Must(x => (x.Type == EnumDeductionType.Percentage && x.Percentage.HasValue && x.Price == null) ||
                      (x.Type == EnumDeductionType.FixedValue && !x.Percentage.HasValue && x.Price != null))
            .WithMessage("When type is Percentage, percentage value is required and price must be null. When type is FixedValue, price is required and percentage must be null");
    }
}