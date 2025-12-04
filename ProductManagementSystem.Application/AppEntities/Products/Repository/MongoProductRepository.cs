using MongoDB.Driver;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Products.Models;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using MongoDB.Bson;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.Products.Repository;

public class MongoProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _productsCollection;
    private readonly ILogger<MongoProductRepository> _logger;


    public MongoProductRepository(IMongoDatabase database, ILogger<MongoProductRepository> logger)
    {
        _productsCollection = database.GetCollection<Product>("products");
        _logger = logger;
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexModels = new[]
        {
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.Name)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.Price.Value)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.Price.Currency)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.ImageUrl)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Descending(p => p.CreatedAt)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.UpdatedAt))
        };
        _productsCollection.Indexes.CreateMany(indexModels);
    }

    public async Task<Product> CreateAsync(Product product)
    {

        _logger.LogInformation("Creating product: {ProductName}", product.Name);

        await _productsCollection.InsertOneAsync(product);

        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
        return product;

    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var product = await _productsCollection.Find(filter).FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", id);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
            throw;
        }
    }
    public async Task<PaginatedResult<Product>> GetAllAsync(PaginationConfigs paginationConfigs, FilterProductDTO? filter = null, string? search = null)
    {
        try
        {
            _logger.LogInformation("Getting all products with pagination: Page {Page}, PageSize {PageSize}, Search {Search}",
                paginationConfigs.Page, paginationConfigs.PageSize, search);

            var filterBuilder = Builders<Product>.Filter;
            var filterDefinition = filterBuilder.Empty;

            // Aplicar filtros si existen
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var nameFilter = filterBuilder.Regex(p => p.Name, new BsonRegularExpression(search, "i"));
                    var imageUrlFilter = filterBuilder.Regex(p => p.ImageUrl, new BsonRegularExpression(search, "i"));
                    filterDefinition = filterBuilder.Or(nameFilter, imageUrlFilter);
                }

                if (filter.MinPrice.HasValue)
                {
                    var minPriceFilter = filterBuilder.Gte(p => p.Price.Value, filter.MinPrice.Value);
                    filterDefinition = filterBuilder.And(filterDefinition, minPriceFilter);
                }

                if (filter.MaxPrice.HasValue)
                {
                    var maxPriceFilter = filterBuilder.Lte(p => p.Price.Value, filter.MaxPrice.Value);
                    filterDefinition = filterBuilder.And(filterDefinition, maxPriceFilter);
                }

                if (filter.Currency.HasValue)
                {
                    var currencyFilter = filterBuilder.Eq(p => p.Price.Currency, filter.Currency.Value);
                    filterDefinition = filterBuilder.And(filterDefinition, currencyFilter);
                }

                if (filter.CreatedAt.HasValue)
                {
                    var createdAtFilter = filterBuilder.Gte(p => p.CreatedAt, filter.CreatedAt.Value);
                    filterDefinition = filterBuilder.And(filterDefinition, createdAtFilter);
                }

                if (filter.UpdatedAt.HasValue)
                {
                    var updatedAtFilter = filterBuilder.Gte(p => p.UpdatedAt, filter.UpdatedAt.Value);
                    filterDefinition = filterBuilder.And(filterDefinition, updatedAtFilter);
                }
            }

            var totalCount = await _productsCollection.CountDocumentsAsync(filterDefinition);

            var skip = (paginationConfigs.Page - 1) * paginationConfigs.PageSize;
            var products = await _productsCollection
                .Find(filterDefinition)
                .Sort(Builders<Product>.Sort.Descending(p => p.CreatedAt))
                .Skip(skip)
                .Limit(paginationConfigs.PageSize)
                .ToListAsync();

            var result = PaginatedResult<Product>.Create(products, (int)totalCount, paginationConfigs.Page, paginationConfigs.PageSize);

            _logger.LogInformation("Retrieved {Count} products out of {TotalCount} total",
                products.Count, totalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw;
        }
    }

    public async Task<Product> UpdateAsync(string id, Product product)
    {
        try
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var update = Builders<Product>.Update
                .Set(p => p.Name, product.Name)
                .Set(p => p.Price, product.Price)
                .Set(p => p.ImageUrl, product.ImageUrl)
                .Set(p => p.Concepts, product.Concepts)
                .Set(p => p.Providers, product.Providers)
                .Set(p => p.Competitors, product.Competitors)
                .Set(p => p.UpdatedAt, product.UpdatedAt);

            var result = await _productsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No product was updated with ID: {ProductId}", id);
                throw new NotFoundException($"Product with ID {id} not found");
            }

            _logger.LogInformation("Product updated successfully with ID: {ProductId}", id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var result = await _productsCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("No product was deleted with ID: {ProductId}", id);
                throw new NotFoundException($"Product with ID {id} not found");
            }

            _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
            throw;
        }
    }

    // Provider operations
    public async Task<Provider> AddProviderAsync(string productId, Provider provider)
    {
        try
        {
            _logger.LogInformation("Adding provider {ProviderName} to product {ProductId}",
                provider.Name, productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update.Push(p => p.Providers, provider);

            var result = await _productsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No product was updated when adding provider. Product ID: {ProductId}", productId);
                throw new NotFoundException($"Product with ID {productId} not found");
            }

            _logger.LogInformation("Provider {ProviderName} added successfully to product {ProductId}",
                provider.Name, productId);
            return provider;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding provider {ProviderName} to product {ProductId}",
                provider.Name, productId);
            throw;
        }
    }

    public async Task RemoveProviderAsync(string productId, string providerName)
    {
        try
        {
            _logger.LogInformation("Removing provider {ProviderName} from product {ProductId}",
                providerName, productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update.PullFilter(p => p.Providers,
                p => p.Name == providerName);

            var result = await _productsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No provider was removed. Product ID: {ProductId}, Provider: {ProviderName}",
                    productId, providerName);
                throw new NotFoundException($"Provider {providerName} not found in product {productId}");
            }

            _logger.LogInformation("Provider {ProviderName} removed successfully from product {ProductId}",
                providerName, productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing provider {ProviderName} from product {ProductId}",
                providerName, productId);
            throw;
        }
    }

    public async Task<List<Provider>> GetProvidersAsync(string productId)
    {
        try
        {
            _logger.LogInformation("Getting providers for product {ProductId}", productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var product = await _productsCollection.Find(filter).FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", productId);
                throw new NotFoundException($"Product with ID {productId} not found");
            }

            _logger.LogInformation("Retrieved {Count} providers for product {ProductId}",
                product.Providers?.Count ?? 0, productId);
            return product.Providers ?? new List<Provider>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting providers for product {ProductId}", productId);
            throw;
        }
    }

    // Concept operations
    public async Task<Concept> AddConceptAsync(string productId, Concept concept)
    {
        try
        {
            _logger.LogInformation("Adding concept {ConceptCode} to product {ProductId}",
                concept.ConceptCode, productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update.Push(p => p.Concepts, concept);

            var result = await _productsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No product was updated when adding concept. Product ID: {ProductId}", productId);
                throw new NotFoundException($"Product with ID {productId} not found");
            }

            _logger.LogInformation("Concept {ConceptCode} added successfully to product {ProductId}",
                concept.ConceptCode, productId);
            return concept;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding concept {ConceptCode} to product {ProductId}",
                concept.ConceptCode, productId);
            throw;
        }
    }

    public async Task RemoveConceptAsync(string productId, string conceptCode)
    {
        try
        {
            _logger.LogInformation("Removing concept {ConceptCode} from product {ProductId}",
                conceptCode, productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update.PullFilter(p => p.Concepts,
                d => d.ConceptCode == conceptCode);

            var result = await _productsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No concept was removed. Product ID: {ProductId}, Concept Code: {ConceptCode}",
                    productId, conceptCode);
                throw new NotFoundException($"Concept {conceptCode} not found in product {productId}");
            }

            _logger.LogInformation("Concept {ConceptCode} removed successfully from product {ProductId}",
                conceptCode, productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing concept {ConceptCode} from product {ProductId}",
                conceptCode, productId);
            throw;
        }
    }

    public async Task<List<Concept>> GetConceptsAsync(string productId)
    {
        try
        {
            _logger.LogInformation("Getting concepts for product {ProductId}", productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var product = await _productsCollection.Find(filter).FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", productId);
                throw new NotFoundException($"Product with ID {productId} not found");
            }

            _logger.LogInformation("Retrieved {Count} concepts for product {ProductId}",
product.Concepts?.Count ?? 0, productId);
            return product.Concepts ?? new List<Concept>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting concepts for product {ProductId}", productId);
            throw;
        }
    }

    // Competitor operations
    public async Task<Competitor> AddCompetitorAsync(string productId, Competitor competitor)
    {
        try
        {
            _logger.LogInformation("Adding competitor with URL {CompetitorUrl} to product {ProductId}",
                competitor.Url, productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update.Push(p => p.Competitors, competitor);

            var result = await _productsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No product was updated when adding competitor. Product ID: {ProductId}", productId);
                throw new NotFoundException($"Product with ID {productId} not found");
            }

            _logger.LogInformation("Competitor with URL {CompetitorUrl} added successfully to product {ProductId}",
                competitor.Url, productId);
            return competitor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding competitor with URL {CompetitorUrl} to product {ProductId}",
                competitor.Url, productId);
            throw;
        }
    }

    public async Task RemoveCompetitorAsync(string productId, string competitorUrl)
    {
        try
        {
            _logger.LogInformation("Removing competitor with URL {CompetitorUrl} from product {ProductId}",
                competitorUrl, productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update.PullFilter(p => p.Competitors,
                c => c.Url == competitorUrl);

            var result = await _productsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No competitor was removed. Product ID: {ProductId}, Competitor URL: {CompetitorUrl}",
                    productId, competitorUrl);
                throw new NotFoundException($"Competitor {competitorUrl} not found in product {productId}");
            }

            _logger.LogInformation("Competitor with URL {CompetitorUrl} removed successfully from product {ProductId}",
                competitorUrl, productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing competitor with URL {CompetitorUrl} from product {ProductId}",
                competitorUrl, productId);
            throw;
        }
    }

    public async Task<List<Competitor>> GetCompetitorsAsync(string productId)
    {
        try
        {
            _logger.LogInformation("Getting competitors for product {ProductId}", productId);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var product = await _productsCollection.Find(filter).FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", productId);
                throw new NotFoundException($"Product with ID {productId} not found");
            }

            _logger.LogInformation("Retrieved {Count} competitors for product {ProductId}",
                product.Competitors?.Count ?? 0, productId);
            return product.Competitors ?? new List<Competitor>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting competitors for product {ProductId}", productId);
            throw;
        }
    }
}