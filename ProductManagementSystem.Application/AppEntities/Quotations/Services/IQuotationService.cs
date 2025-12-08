using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Type;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Services;

public interface IQuotationService
{
    Task<QuotationDTO> CreateAsync(CreateQuotationDTO dto);
    Task<QuotationDTO?> GetByIdAsync(string id);
    Task<PaginatedResult<QuotationDTO>> GetByProductIdAsync(string productId, PaginationConfigDTO paginationConfigs, FilterQuotationDTO? filter = null);
    Task<List<QuotationDTO>> GetAllByProductIdAsync(string productId);
    Task<QuotationDTO> UpdateAsync(string id, UpdateQuotationDTO dto);
    Task DeleteAsync(string id);
    Task DeleteByProductIdAsync(string productId);
}

