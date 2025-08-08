using ProductManagementSystem.Application.Domain.GlobalParameters.Models;
using ProductManagementSystem.Application.Domain.Shared.Type;

namespace ProductManagementSystem.Application.Domain.GlobalParameters.Repository;

public interface IGlobalParametersRepository
{
    Task<Concept> CreateAsync(Concept concept);
    Task<List<Concept>> GetAllAsync();
    Task<Concept?> GetAsync(string conceptCode);
    Task<Concept?> UpdateAsync(Concept concept);

}