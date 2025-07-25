using MongoDB.Driver;
using ProductManagementSystem.Application.Domain.Subscriptions.Enums;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Requests;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Repository;

public class MongoSubscriptionRepository : ISubscriptionRepository
{
    private readonly IMongoCollection<Subscription> _collection;

    public MongoSubscriptionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Subscription>("subscriptions");
    }

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        await _collection.InsertOneAsync(subscription);
        return subscription;
    }

    public async Task<Subscription?> GetByIdAsync(string id)
    {
        return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<PaginatedResult<Subscription>> GetAllAsync(SubscriptionFilterDTO filter)
    {
        // Construir filtros de búsqueda
        var filterDefinition = Builders<Subscription>.Filter.Empty;
        var filterBuilder = Builders<Subscription>.Filter;

        if (!string.IsNullOrEmpty(filter.Name))
            filterDefinition &= filterBuilder.Regex(s => s.Name, new MongoDB.Bson.BsonRegularExpression(filter.Name, "i"));

        if (!string.IsNullOrEmpty(filter.Period))
        {
            if (Enum.TryParse<EnumSubscriptionPeriod>(filter.Period, true, out var periodEnum))
            {
                filterDefinition &= filterBuilder.Eq(s => s.Period, periodEnum);
            }
        }

        if (filter.IsActive.HasValue)
            filterDefinition &= filterBuilder.Eq(s => s.IsActive, filter.IsActive.Value);

        var skip = (filter.Page - 1) * filter.PageSize;

        var totalCount = await _collection.CountDocumentsAsync(filterDefinition);
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var subscriptions = await _collection.Find(filterDefinition)
            .Skip(skip)
            .Limit(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<Subscription>
        {
            Items = subscriptions,
            TotalCount = (int)totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(s => s.Id == id);
    }
}