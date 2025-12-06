using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Models;

namespace ProductManagementSystem.Application.AppEntities.ConceptCodes.Repository;

public interface IConceptCodeRepository
{
    Task<ConceptCode?> GetByCodeAsync(string code);
    Task<List<ConceptCode>> GetAllAsync();
    Task<PaginatedResult<ConceptCode>> GetAllAsync(PaginationConfigs paginationConfigs, FilterConceptCodeDTO filter);
    Task<ConceptCode> CreateAsync(ConceptCode conceptCode);
    Task<ConceptCode> UpdateAsync(ConceptCode conceptCode);
    Task<bool> ExistsByCodeAsync(string code);
}