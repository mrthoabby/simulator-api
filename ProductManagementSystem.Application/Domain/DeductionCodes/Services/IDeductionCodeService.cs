using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Outputs;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.Services;

public interface IDeductionCodeService
{
    Task<DeductionCodeDTO?> GetByIdAsync(string id);
    Task<DeductionCodeDTO?> GetByCodeAsync(string code);
    Task<List<DeductionCodeDTO>> GetAllAsync();
    Task<PaginatedResult<DeductionCodeDTO>> GetFilteredAsync(PaginationConfigs paginationConfigs, FilterDeductionCodeDTO filter);
    Task<DeductionCodeDTO> CreateAsync(CreateDeductionCodeDTO request);
    Task<DeductionCodeDTO> UpdateAsync(string code, UpdateDeductionCodeDTO request);
    Task<bool> DeleteAsync(string id);

    Task<bool> ExistsByCodeAsync(string code);
    Task<List<DeductionCodeDTO>> SearchByPatternAsync(string pattern);
    Task<string> GenerateNextCodeAsync(string prefix);
}