using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.Concepts.Domain;

public interface IConceptDomainRules
{
    Task Validate(List<Concept> concepts);
    Task Validate(Concept concept);
}
