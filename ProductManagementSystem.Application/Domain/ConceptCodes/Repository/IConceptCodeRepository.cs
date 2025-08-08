using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.ConceptCodes.Models;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.Repository;

public interface IConceptCodeRepository
{
    Task<ConceptCode?> GetByCodeAsync(string code);
    Task<List<ConceptCode>> GetAllAsync();
    Task<PaginatedResult<ConceptCode>> GetAllAsync(PaginationConfigs paginationConfigs, FilterConceptCodeDTO filter);
    Task<ConceptCode> CreateAsync(ConceptCode conceptCode);
    Task<ConceptCode> UpdateAsync(ConceptCode conceptCode);
    Task<bool> ExistsByCodeAsync(string code);
}