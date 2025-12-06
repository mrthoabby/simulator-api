using FluentValidation;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;
using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.Shared.Type;

public class Concept : Entity
{
    public string ConceptCode { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public EnumConceptType Type { get; private set; }
    public decimal? Percentage { get; private set; }
    public Money? Price { get; private set; }
    public EnumConceptApplication Application { get; private set; }

    protected Concept(string conceptCode, string name, EnumConceptApplication application, Money money, string? description = null) : base()
    {
        ConceptCode = conceptCode;
        Name = name;
        Application = application;
        Description = description;
        Price = money;
        Percentage = null;
        Type = EnumConceptType.FixedValue;
    }

    protected Concept(string conceptCode, string name, EnumConceptApplication application, string? description = null) : base()
    {
        ConceptCode = conceptCode;
        Name = name;
        Application = application;
        Description = description;
    }

    protected Concept(string conceptCode, string name, EnumConceptApplication application, decimal percentage, string? description = null) : base()
    {
        ConceptCode = conceptCode;
        Name = name;
        Application = application;
        Description = description;
        Percentage = percentage;
        Price = null;
        Type = EnumConceptType.Percentage;
    }

    public static Concept Create(string conceptCode, string name, EnumConceptApplication application, decimal percentage, string? description = null)
    {
        var deduction = new Concept(conceptCode, name, application, percentage, description);

        var validator = new DeductionValidator();
        var validationResult = validator.Validate(deduction);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return deduction;
    }

    public static Concept Create(string conceptCode, string name, EnumConceptApplication application, Money price, string? description = null)
    {
        var deduction = new Concept(conceptCode, name, application, price, description);

        var validator = new DeductionValidator();
        var validationResult = validator.Validate(deduction);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return deduction;
    }

    public static Concept Create(string conceptCode, string name, EnumConceptApplication application, string? description = null)
    {
        var deduction = new Concept(conceptCode, name, application, description);

        var validator = new DeductionValidator();
        var validationResult = validator.Validate(deduction);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return deduction;
    }
    public void SetName(string name)
    {
        Name = name;
    }
    public void SetDescription(string description)
    {
        Description = description;
    }
    public void SetApplication(EnumConceptApplication application)
    {
        Application = application;
    }
    public void SetType(EnumConceptType type)
    {
        Type = type;
    }
    public void SetPrice(Money price)
    {
        Price = price;
    }
    public void SetPercentage(decimal percentage)
    {
        Percentage = percentage;
    }

}

public class DeductionValidator : AbstractValidator<Concept>
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
            .Must(x => (x.Type == EnumConceptType.Percentage && x.Percentage.HasValue && x.Price == null) ||
                      (x.Type == EnumConceptType.FixedValue && !x.Percentage.HasValue && x.Price != null))
            .WithMessage("When type is Percentage, percentage value is required and price must be null. When type is FixedValue, price is required and percentage must be null");
    }
}
