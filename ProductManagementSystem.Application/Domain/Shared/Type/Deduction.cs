using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Enum;
using ProductManagementSystem.Application.Domain.Shared.Type.Prices;

namespace ProductManagementSystem.Application.Domain.Shared.Type;

public class Deduction
{
    public string ConceptCode { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public EnumDeductionType DeductionType { get; private set; }
    public decimal? Percentage { get; private set; }
    public Price? Price { get; private set; }
    public EnumDeductionApplication Application { get; private set; }

    protected Deduction(string conceptCode, string name, EnumDeductionApplication application, string? description = null)
    {
        ConceptCode = conceptCode;
        Name = name;
        Application = application;
        Description = description;
    }

    public static DeductionBuilder CreateBuilder(string conceptCode, string name, EnumDeductionApplication application, string? description = null)
    {
        return new DeductionBuilder(conceptCode, name, application, description);
    }


    public class DeductionBuilder
    {
        private readonly Deduction _deduction;

        public DeductionBuilder(string conceptCode, string name, EnumDeductionApplication application, string? description = null)
        {
            _deduction = new Deduction(conceptCode, name, application, description);
        }

        public DeductionBuilder WithPercentage(decimal percentage)
        {
            _deduction.Percentage = percentage;
            _deduction.Price = null;
            _deduction.DeductionType = EnumDeductionType.Percentage;
            return this;
        }

        public DeductionBuilder WithPrice(Price price)
        {
            _deduction.Price = price;
            _deduction.Percentage = null;
            _deduction.DeductionType = EnumDeductionType.FixedValue;
            return this;
        }

        public Deduction Build()
        {
            var validator = new DeductionValidator();
            var validationResult = validator.Validate(_deduction);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                throw new ValidationException(string.Join(", ", errors));
            }

            return _deduction;
        }
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
            .Must(x => (x.Percentage.HasValue && x.Price == null) || (!x.Percentage.HasValue && x.Price != null))
            .WithMessage("Deduction must have either Percentage or Price, but not both or neither");
    }
}