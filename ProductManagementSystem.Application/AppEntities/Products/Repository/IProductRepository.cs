using ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Products.Models;
using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.Products.Repository;

public interface IProductRepository
{
    Task<Product> CreateAsync(Product product);
    Task<Product?> GetByIdAsync(string id);
    Task<PaginatedResult<Product>> GetAllAsync(PaginationConfigs paginationConfigs, FilterProductDTO? filter = null, string? search = null);
    Task<Product> UpdateAsync(string id, Product product);
    Task DeleteAsync(string id);

    // Provider operations
    Task<Provider> AddProviderAsync(string productId, Provider provider);
    Task RemoveProviderAsync(string productId, string providerName);
    Task<List<Provider>> GetProvidersAsync(string productId);

    // Concept operations
    Task<Concept> AddConceptAsync(string productId, Concept concept);
    Task RemoveConceptAsync(string productId, string conceptCode);
    Task<List<Concept>> GetConceptsAsync(string productId);

    // Competitor operations
    Task<Competitor> AddCompetitorAsync(string productId, Competitor competitor);
    Task RemoveCompetitorAsync(string productId, string competitorUrl);
    Task<List<Competitor>> GetCompetitorsAsync(string productId);
}