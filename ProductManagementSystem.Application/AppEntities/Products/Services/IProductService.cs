using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;


namespace ProductManagementSystem.Application.AppEntities.Products.Services;

public interface IProductService
{
    // Product operations
    Task<ProductDTO> CreateAsync(CreateProductDTO dto);
    Task<ProductDTO?> GetByIdAsync(string id);
    Task<PaginatedResult<ProductDTO>> GetAllAsync(PaginationConfigDTO paginationConfigs, FilterProductDTO? filter = null, string? search = null);
    Task<ProductDTO> UpdateAsync(string id, UpdateProductDTO dto);
    Task<ProductDTO> UpdateImageAsync(string id, UpdateProductImageDTO dto);
    Task DeleteAsync(string id);

    // Concept operations
    Task<ConceptDTO> AddConceptAsync(string productId, AddConceptDTO dto);
    Task RemoveConceptAsync(string productId, string conceptCode);
    Task<List<ConceptDTO>> GetConceptsAsync(string productId);

    // Provider operations
    Task<ProviderDTO> AddProviderAsync(string productId, AddProviderDTO dto);
    Task RemoveProviderAsync(string productId, string providerName);
    Task<List<ProviderDTO>> GetProvidersAsync(string productId);

    // Competitor operations
    Task<CompetitorDTO> AddCompetitorAsync(string productId, AddCompetitorDTO dto);
    Task RemoveCompetitorAsync(string productId, string competitorUrl);
    Task<List<CompetitorDTO>> GetCompetitorsAsync(string productId);
}