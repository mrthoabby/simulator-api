using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.DeductionCodes.Services;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Shared.Type;

public class DeductionDomainRules : IDeductionDomainRules
{
    private readonly IDeductionCodeService _deductionCodeService;

    public DeductionDomainRules(IDeductionCodeService deductionCodeService)
    {
        _deductionCodeService = deductionCodeService;
    }

    private async Task<bool> hasValidConceptCode(List<Deduction> deductions)
    {
        var deductionCodes = await _deductionCodeService.GetAllAsync();
        foreach (var deduction in deductions)
        {
            if (!deductionCodes.Any(dc => dc.Code == deduction.ConceptCode))
            {
                throw new NotFoundException($"Deduction with concept code {deduction.ConceptCode} has an invalid concept code");
            }
        }
        return true;
    }


    public async Task Validate(List<Deduction> deductions)
    {
        await hasValidConceptCode(deductions);
    }

    public Task Validate(Deduction deduction)
    {
        throw new NotImplementedException();
    }
}