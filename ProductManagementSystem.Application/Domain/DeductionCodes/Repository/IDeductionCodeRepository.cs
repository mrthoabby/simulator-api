using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.Models;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.Repository;

public interface IDeductionCodeRepository
{
    Task<DeductionCode?> GetByIdAsync(string id);
    Task<DeductionCode?> GetByCodeAsync(string code);
    Task<List<DeductionCode>> GetAllAsync();
    Task<PaginatedResult<DeductionCode>> GetFilteredAsync(PaginationConfigs paginationConfigs, FilterDeductionCodeDTO filter);
    Task<DeductionCode> CreateAsync(DeductionCode deductionCode);
    Task<DeductionCode> UpdateAsync(DeductionCode deductionCode);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsByCodeAsync(string code);
    Task<List<DeductionCode>> SearchByPatternAsync(string pattern);
    Task<List<DeductionCode>> GetByPrefixAsync(string prefix);
}