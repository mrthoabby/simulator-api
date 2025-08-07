using FluentValidation;
using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.Models;

public class DeductionCode : Entity
{
    public string Code { get; set; }
    public bool IsFromSystem { get; set; }


    private DeductionCode(string code, bool isFromSystem) : base()
    {
        Code = code.ToUpper();
        IsFromSystem = isFromSystem;
        UpdateTimestamp();
    }

    public static DeductionCode Create(string code, bool isFromSystem = false)
    {
        var deductionCode = new DeductionCode(code, isFromSystem);

        var validator = new DeductionCodeValidator();
        var validationResult = validator.Validate(deductionCode);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }

        return deductionCode;
    }
}

public class DeductionCodeValidator : AbstractValidator<DeductionCode>
{
    public DeductionCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Deduction code is required")
            .MaximumLength(50).WithMessage("Deduction code cannot exceed 50 characters")
            .Matches(@"^[A-Z_]+$").WithMessage("Deduction code must contain only uppercase letters and underscores");
    }
}