using FluentValidation;
using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.Models;

public class ConceptCode : Entity
{
    public string Code { get; set; }
    public bool IsFromSystem { get; set; }

    private ConceptCode(string code, bool isFromSystem) : base()
    {
        Code = code.ToUpper();
        IsFromSystem = isFromSystem;
        UpdateTimestamp();
    }

    public static ConceptCode Create(string code, bool isFromSystem)
    {
        var conceptCode = new ConceptCode(code, isFromSystem);

        var validator = new ConceptCodeValidator();
        var validationResult = validator.Validate(conceptCode);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return conceptCode;
    }
}

public class ConceptCodeValidator : AbstractValidator<ConceptCode>
{
    public ConceptCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Concept code is required")
            .MaximumLength(50).WithMessage("Concept code cannot exceed 50 characters");
    }
}