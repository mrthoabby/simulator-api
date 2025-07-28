using MongoDB.Driver;
using ProductManagementSystem.Application.Domain.Subscriptions.Enums;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Repository;

public class MongoSubscriptionRepository : ISubscriptionRepository
{
    private readonly IMongoCollection<Subscription> _collection;
    private readonly ILogger<MongoSubscriptionRepository> _logger;

    public MongoSubscriptionRepository(IMongoDatabase database, ILogger<MongoSubscriptionRepository> logger)
    {
        _collection = database.GetCollection<Subscription>("subscriptions");
        _logger = logger;
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        // Índice para búsquedas por nombre (case-insensitive)
        var nameIndexKeys = Builders<Subscription>.IndexKeys.Ascending(s => s.Name);
        var nameIndexOptions = new CreateIndexOptions
        {
            Name = "idx_subscription_name",
            Collation = new MongoDB.Driver.Collation("en", strength: MongoDB.Driver.CollationStrength.Secondary)
        };
        _collection.Indexes.CreateOne(new CreateIndexModel<Subscription>(nameIndexKeys, nameIndexOptions));

        // Índice para filtros por período
        var periodIndexKeys = Builders<Subscription>.IndexKeys.Ascending(s => s.Period);
        var periodIndexOptions = new CreateIndexOptions { Name = "idx_subscription_period" };
        _collection.Indexes.CreateOne(new CreateIndexModel<Subscription>(periodIndexKeys, periodIndexOptions));

        // Índice para filtros por estado activo
        var isActiveIndexKeys = Builders<Subscription>.IndexKeys.Ascending(s => s.IsActive);
        var isActiveIndexOptions = new CreateIndexOptions { Name = "idx_subscription_isactive" };
        _collection.Indexes.CreateOne(new CreateIndexModel<Subscription>(isActiveIndexKeys, isActiveIndexOptions));

        // Índice compuesto para consultas comunes (IsActive + Period)
        var compositeIndexKeys = Builders<Subscription>.IndexKeys
            .Ascending(s => s.IsActive)
            .Ascending(s => s.Period);
        var compositeIndexOptions = new CreateIndexOptions { Name = "idx_subscription_active_period" };
        _collection.Indexes.CreateOne(new CreateIndexModel<Subscription>(compositeIndexKeys, compositeIndexOptions));

        // Índice único para nombre (evitar duplicados)
        var uniqueNameIndexKeys = Builders<Subscription>.IndexKeys.Ascending(s => s.Name);
        var uniqueNameIndexOptions = new CreateIndexOptions
        {
            Name = "idx_subscription_name_unique",
            Unique = true,
            Collation = new MongoDB.Driver.Collation("en", strength: MongoDB.Driver.CollationStrength.Secondary)
        };
        try
        {
            _collection.Indexes.CreateOne(new CreateIndexModel<Subscription>(uniqueNameIndexKeys, uniqueNameIndexOptions));
        }
        catch (MongoCommandException)
        {
            _logger.LogWarning("Index already exists or there are duplicate data, it can be ignored");
        }
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

    public async Task<Subscription?> GetByNameAsync(string name)
    {
        return await _collection.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefaultAsync();
    }

    public async Task<PaginatedResult<Subscription>> GetAllAsync(SubscriptionFilterDTO filter)
    {
        // Construir filtros de búsqueda
        var filterDefinition = Builders<Subscription>.Filter.Empty;
        var filterBuilder = Builders<Subscription>.Filter;

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            // Limitar longitud del regex para evitar ataques DoS
            var safeName = filter.Name.Length > 100 ? filter.Name[..100] : filter.Name;
            var escapedName = System.Text.RegularExpressions.Regex.Escape(safeName);
            filterDefinition &= filterBuilder.Regex(s => s.Name, new MongoDB.Bson.BsonRegularExpression(escapedName, "i"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Period))
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