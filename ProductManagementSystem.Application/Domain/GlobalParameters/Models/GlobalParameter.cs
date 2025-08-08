using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Shared.Enum;
using ProductManagementSystem.Application.Domain.Shared.Type;

namespace ProductManagementSystem.Application.Domain.GlobalParameters.Models;

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