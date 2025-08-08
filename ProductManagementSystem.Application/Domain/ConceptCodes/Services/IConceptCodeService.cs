using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Shared.DTOs;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.Services;

public interface IConceptCodeService
{
    Task<ConceptCodeDTO?> GetByCodeAsync(string code);
    Task<PaginatedResult<ConceptCodeDTO>> GetAllAsync(PaginationConfigDTO paginationConfigs, FilterConceptCodeDTO filter);
    Task<List<ConceptCodeDTO>> GetAllAsync();
    Task<ConceptCodeDTO> CreateAsync(CreateConceptCodeDTO createDto);
    Task<ConceptCodeDTO> UpdateAsync(string code, UpdateConceptCodeDTO updateDto);
    Task<bool> ExistsByCodeAsync(string code);
}