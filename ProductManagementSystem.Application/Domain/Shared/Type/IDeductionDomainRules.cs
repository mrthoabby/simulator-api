namespace ProductManagementSystem.Application.Domain.Shared.Type;

public interface IDeductionDomainRules
{
    Task Validate(List<Deduction> deductions);
    Task Validate(Deduction deduction);
}