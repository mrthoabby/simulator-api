using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.Models;
using ProductManagementSystem.Application.Common.AppEntities.Type;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Repository;

public interface IQuotationRepository
{
    Task<Quotation> CreateAsync(Quotation quotation);
    Task<Quotation?> GetByIdAsync(string id);
    Task<PaginatedResult<Quotation>> GetByProductIdAsync(string productId, PaginationConfigs paginationConfigs, FilterQuotationDTO? filter = null);
    Task<List<Quotation>> GetAllByProductIdAsync(string productId);
    Task<Quotation> UpdateAsync(string id, Quotation quotation);
    Task DeleteAsync(string id);
    Task DeleteByProductIdAsync(string productId);
}

