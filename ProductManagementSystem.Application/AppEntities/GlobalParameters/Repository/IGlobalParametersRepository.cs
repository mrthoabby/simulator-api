using ProductManagementSystem.Application.AppEntities.GlobalParameters.Models;
using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.GlobalParameters.Repository;

public interface IGlobalParametersRepository
{
    Task<Concept> CreateAsync(Concept concept);
    Task<List<Concept>> GetAllAsync();
    Task<Concept?> GetAsync(string conceptCode);
    Task<Concept?> UpdateAsync(Concept concept);

}