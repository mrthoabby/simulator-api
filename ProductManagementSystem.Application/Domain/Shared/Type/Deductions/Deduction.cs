using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Shared.Type.Deductions;

public abstract class Deduction
{
    public string ConceptCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public EnumDeductionType Type { get; init; }
    public EnumDeductionApplication Application { get; init; }

    protected Deduction(string conceptCode, string name, EnumDeductionType type, EnumDeductionApplication application, string? description = null)
    {
        ConceptCode = conceptCode;
        Name = name;
        Type = type;
        Application = application;
        Description = description;

        var validator = new DeductionValidator();
        var validationResult = validator.Validate(this);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
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

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Deduction type must be a valid enum value");

        RuleFor(x => x.Application)
            .IsInEnum().WithMessage("Deduction application must be a valid enum value");
    }
}