using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;
using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.GlobalParameters.Models;

public class GlobalParameter : Concept
{
    public GlobalParameter(string conceptCode, string name, EnumConceptApplication application, Money price, string? description = null)
    : base(conceptCode, name, application, price, description)
    {
    }
    public GlobalParameter(string conceptCode, string name, EnumConceptApplication application, decimal percentage, string? description = null)
    : base(conceptCode, name, application, percentage, description)
    {
    }
}