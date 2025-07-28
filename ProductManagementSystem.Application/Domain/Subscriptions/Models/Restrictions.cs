using FluentValidation;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Models;

public class Restrictions
{
    public int MaxProducts { get; init; }
    public int MaxUsers { get; init; }
    public int MaxCompetitors { get; init; }
    public int MaxCustomDeductions { get; init; }
    public int MaxSimulations { get; init; }

    public bool IsPDFExportSupported { get; init; }
    public bool IsSimulationComparisonSupported { get; init; }
    public bool IsExcelExportSupported { get; init; }

    private Restrictions()
    {
    }

    private Restrictions(int maxProducts, int maxUsers, int maxCompetitors, int maxCustomDeductions, int maxSimulations, bool isPDFExportSupported, bool isSimulationComparisonSupported, bool isExcelExportSupported)
    {
        MaxProducts = maxProducts;
        MaxUsers = maxUsers;
        MaxCompetitors = maxCompetitors;
        MaxCustomDeductions = maxCustomDeductions;
        MaxSimulations = maxSimulations;
        IsPDFExportSupported = isPDFExportSupported;
        IsSimulationComparisonSupported = isSimulationComparisonSupported;
        IsExcelExportSupported = isExcelExportSupported;

        var validator = new RestrictionsValidator();
        var validationResult = validator.Validate(this);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Restrictions validation failed: {errors}");
        }
    }

    public static Restrictions Create(int maxProducts, int maxUsers, int maxCompetitors, int maxCustomDeductions, int maxSimulations, bool isPDFExportSupported, bool isSimulationComparisonSupported, bool isExcelExportSupported)
    {
        return new Restrictions(maxProducts, maxUsers, maxCompetitors, maxCustomDeductions, maxSimulations, isPDFExportSupported, isSimulationComparisonSupported, isExcelExportSupported);
    }
}

public class RestrictionsValidator : AbstractValidator<Restrictions>
{
    public RestrictionsValidator()
    {
        RuleFor(x => x.MaxProducts)
            .GreaterThanOrEqualTo(0).WithMessage("Max products must be greater than or equal to 0");

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0).WithMessage("Max users must be greater than 0");

        RuleFor(x => x.MaxCompetitors)
            .GreaterThanOrEqualTo(0).WithMessage("Max competitors must be greater than or equal to 0");

        RuleFor(x => x.MaxCustomDeductions)
            .GreaterThanOrEqualTo(0).WithMessage("Max custom deductions must be greater than or equal to 0");

        RuleFor(x => x.MaxSimulations)
            .GreaterThanOrEqualTo(0).WithMessage("Max simulations must be greater than or equal to 0");
    }
}
