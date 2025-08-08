namespace ProductManagementSystem.Application.Domain.Shared.Type;

public interface IConceptDomainRules
{
    Task Validate(List<Concept> concepts);
    Task Validate(Concept concept);
}