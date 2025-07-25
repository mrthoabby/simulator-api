using FluentValidation;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Models;

public class Restrictions
{
    public int MaxProducts { get; set; }
    public int MaxUsers { get; set; }
    public int MaxCompetitors { get; set; }
    public int MaxCustomDeductions { get; set; }
    public int MaxSimulations { get; set; }

    public bool IsPDFExportSupported { get; set; }
    public bool IsSimulationComparisonSupported { get; set; }
    public bool IsExcelExportSupported { get; set; }

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
