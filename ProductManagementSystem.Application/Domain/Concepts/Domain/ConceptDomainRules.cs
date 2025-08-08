using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.ConceptCodes.Services;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Shared.Type;

public class ConceptDomainRules : IConceptDomainRules
{
    private readonly IConceptCodeService _deductionCodeService;

    public ConceptDomainRules(IConceptCodeService deductionCodeService)
    {
        _deductionCodeService = deductionCodeService;
    }

    private async Task<bool> hasValidConceptCode(List<Concept> deductions)
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


    public async Task Validate(List<Concept> deductions)
    {
        await hasValidConceptCode(deductions);
    }

    public Task Validate(Concept deduction)
    {
        throw new NotImplementedException();
    }
}