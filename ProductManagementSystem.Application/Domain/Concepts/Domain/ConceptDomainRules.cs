using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.ConceptCodes.Services;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Shared.Type;

public class ConceptDomainRules : IConceptDomainRules
{
    private readonly IConceptCodeService _conceptCodeService;

    public ConceptDomainRules(IConceptCodeService conceptCodeService)
    {
        _conceptCodeService = conceptCodeService;
    }

    private async Task<bool> hasValidConceptCode(List<Concept> concepts)
    {
        var conceptCodes = await _conceptCodeService.GetAllAsync();
        foreach (var concept in concepts)
        {
            if (!conceptCodes.Any(cc => cc.Code.ToUpper() == concept.ConceptCode.ToUpper()))
            {
                throw new NotFoundException($"Concept with concept code {concept.ConceptCode} has an invalid concept code");
            }
        }
        return true;
    }


    public async Task Validate(List<Concept> concepts)
    {
        await hasValidConceptCode(concepts);
    }

    public Task Validate(Concept concept)
    {
        throw new NotImplementedException();
    }
}