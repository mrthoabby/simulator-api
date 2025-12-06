using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;

namespace ProductManagementSystem.Application.AppEntities.ConceptCodes.Services;

public interface IConceptCodeService
{
    Task<ConceptCodeDTO?> GetByCodeAsync(string code);
    Task<PaginatedResult<ConceptCodeDTO>> GetAllAsync(PaginationConfigDTO paginationConfigs, FilterConceptCodeDTO filter);
    Task<List<ConceptCodeDTO>> GetAllAsync();
    Task<ConceptCodeDTO> CreateAsync(CreateConceptCodeDTO createDto);
    Task<ConceptCodeDTO> UpdateAsync(string code, UpdateConceptCodeDTO updateDto);
    Task<bool> ExistsByCodeAsync(string code);
}