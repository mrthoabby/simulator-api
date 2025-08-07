using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;


namespace ProductManagementSystem.Application.Domain.Products.Services;

public interface IProductService
{
    // Product operations
    Task<ProductDTO> CreateAsync(CreateProductDTO request);
    Task<ProductDTO?> GetByIdAsync(string id);
    Task<PaginatedResult<ProductDTO>> GetAllAsync(PaginationConfigs paginationConfigs, FilterProductDTO? filter = null, string? search = null);
    Task<ProductDTO> UpdateAsync(string id, UpdateProductDTO request);
    Task<ProductDTO> UpdateImageAsync(string id, UpdateProductImageDTO request);
    Task DeleteAsync(string id);

    // Deduction operations
    Task<DeductionDTO> AddDeductionAsync(string productId, AddDeductionDTO request);
    Task RemoveDeductionAsync(string productId, string conceptCode);
    Task<List<DeductionDTO>> GetDeductionsAsync(string productId);

    // Provider operations
    Task<ProviderDTO> AddProviderAsync(string productId, AddProviderDTO request);
    Task RemoveProviderAsync(string productId, string providerName);
    Task<List<ProviderDTO>> GetProvidersAsync(string productId);

    // Competitor operations
    Task<CompetitorDTO> AddCompetitorAsync(string productId, AddCompetitorDTO request);
    Task RemoveCompetitorAsync(string productId, string competitorUrl);
    Task<List<CompetitorDTO>> GetCompetitorsAsync(string productId);
}