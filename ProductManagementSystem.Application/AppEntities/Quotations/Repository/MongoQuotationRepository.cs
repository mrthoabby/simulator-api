using MongoDB.Driver;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.Models;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Repository;

public class MongoQuotationRepository : IQuotationRepository
{
    private readonly IMongoCollection<Quotation> _quotationsCollection;
    private readonly ILogger<MongoQuotationRepository> _logger;

    public MongoQuotationRepository(IMongoDatabase database, ILogger<MongoQuotationRepository> logger)
    {
        _quotationsCollection = database.GetCollection<Quotation>("quotations");
        _logger = logger;
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexModels = new[]
        {
            new CreateIndexModel<Quotation>(Builders<Quotation>.IndexKeys.Ascending(q => q.ProductId)),
            new CreateIndexModel<Quotation>(Builders<Quotation>.IndexKeys.Ascending(q => q.ProviderId)),
            new CreateIndexModel<Quotation>(Builders<Quotation>.IndexKeys.Ascending(q => q.IsActive)),
            new CreateIndexModel<Quotation>(Builders<Quotation>.IndexKeys.Descending(q => q.CreatedAt))
        };
        _quotationsCollection.Indexes.CreateMany(indexModels);
    }

    public async Task<Quotation> CreateAsync(Quotation quotation)
    {
        _logger.LogInformation("Creating quotation for product {ProductId} with provider {ProviderId}", 
            quotation.ProductId, quotation.ProviderId);

        await _quotationsCollection.InsertOneAsync(quotation);

        _logger.LogInformation("Quotation created successfully with ID: {QuotationId}", quotation.Id);
        return quotation;
    }

    public async Task<Quotation?> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting quotation by ID: {QuotationId}", id);

            var filter = Builders<Quotation>.Filter.Eq(q => q.Id, id);
            var quotation = await _quotationsCollection.Find(filter).FirstOrDefaultAsync();

            if (quotation == null)
            {
                _logger.LogWarning("Quotation not found with ID: {QuotationId}", id);
            }

            return quotation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotation by ID: {QuotationId}", id);
            throw;
        }
    }

    public async Task<PaginatedResult<Quotation>> GetByProductIdAsync(string productId, PaginationConfigs paginationConfigs, FilterQuotationDTO? filter = null)
    {
        try
        {
            _logger.LogInformation("Getting quotations for product {ProductId} with pagination: Page {Page}, PageSize {PageSize}",
                productId, paginationConfigs.Page, paginationConfigs.PageSize);

            var filterBuilder = Builders<Quotation>.Filter;
            var filterDefinition = filterBuilder.Eq(q => q.ProductId, productId);

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.ProviderId))
                {
                    filterDefinition = filterBuilder.And(filterDefinition, 
                        filterBuilder.Eq(q => q.ProviderId, filter.ProviderId));
                }

                if (filter.IsActive.HasValue)
                {
                    filterDefinition = filterBuilder.And(filterDefinition, 
                        filterBuilder.Eq(q => q.IsActive, filter.IsActive.Value));
                }

                if (filter.CreatedAt.HasValue)
                {
                    filterDefinition = filterBuilder.And(filterDefinition, 
                        filterBuilder.Gte(q => q.CreatedAt, filter.CreatedAt.Value));
                }
            }

            var totalCount = await _quotationsCollection.CountDocumentsAsync(filterDefinition);

            var skip = (paginationConfigs.Page - 1) * paginationConfigs.PageSize;
            var quotations = await _quotationsCollection
                .Find(filterDefinition)
                .Sort(Builders<Quotation>.Sort.Descending(q => q.CreatedAt))
                .Skip(skip)
                .Limit(paginationConfigs.PageSize)
                .ToListAsync();

            var result = PaginatedResult<Quotation>.Create(quotations, (int)totalCount, paginationConfigs.Page, paginationConfigs.PageSize);

            _logger.LogInformation("Retrieved {Count} quotations out of {TotalCount} total for product {ProductId}",
                quotations.Count, totalCount, productId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotations for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<List<Quotation>> GetAllByProductIdAsync(string productId)
    {
        try
        {
            _logger.LogInformation("Getting all quotations for product {ProductId}", productId);

            var filter = Builders<Quotation>.Filter.Eq(q => q.ProductId, productId);
            var quotations = await _quotationsCollection
                .Find(filter)
                .Sort(Builders<Quotation>.Sort.Descending(q => q.CreatedAt))
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} quotations for product {ProductId}", quotations.Count, productId);

            return quotations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all quotations for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<Quotation> UpdateAsync(string id, Quotation quotation)
    {
        try
        {
            _logger.LogInformation("Updating quotation with ID: {QuotationId}", id);

            var filter = Builders<Quotation>.Filter.Eq(q => q.Id, id);
            var update = Builders<Quotation>.Update
                .Set(q => q.Dimensions, quotation.Dimensions)
                .Set(q => q.UnitsPerBox, quotation.UnitsPerBox)
                .Set(q => q.TotalUnits, quotation.TotalUnits)
                .Set(q => q.IsActive, quotation.IsActive)
                .Set(q => q.UpdatedAt, quotation.UpdatedAt);

            var result = await _quotationsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                _logger.LogWarning("No quotation was updated with ID: {QuotationId}", id);
                throw new NotFoundException($"Quotation with ID {id} not found");
            }

            _logger.LogInformation("Quotation updated successfully with ID: {QuotationId}", id);
            return quotation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quotation with ID: {QuotationId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting quotation with ID: {QuotationId}", id);

            var filter = Builders<Quotation>.Filter.Eq(q => q.Id, id);
            var result = await _quotationsCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("No quotation was deleted with ID: {QuotationId}", id);
                throw new NotFoundException($"Quotation with ID {id} not found");
            }

            _logger.LogInformation("Quotation deleted successfully with ID: {QuotationId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quotation with ID: {QuotationId}", id);
            throw;
        }
    }

    public async Task DeleteByProductIdAsync(string productId)
    {
        try
        {
            _logger.LogInformation("Deleting all quotations for product {ProductId}", productId);

            var filter = Builders<Quotation>.Filter.Eq(q => q.ProductId, productId);
            var result = await _quotationsCollection.DeleteManyAsync(filter);

            _logger.LogInformation("Deleted {Count} quotations for product {ProductId}", result.DeletedCount, productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quotations for product {ProductId}", productId);
            throw;
        }
    }
}

