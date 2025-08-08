using MongoDB.Driver;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.ConceptCodes.Models;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.Repository;

public class ConceptCodeRepository : IConceptCodeRepository
{
    private readonly IMongoCollection<ConceptCode> _collection;
    private const string CollectionName = "concept_codes";

    public ConceptCodeRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConceptCode>(CollectionName);
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var codeIndex = Builders<ConceptCode>.IndexKeys.Ascending(x => x.Code);
        var codeOptions = new CreateIndexOptions { Unique = true };
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<ConceptCode>(codeIndex, codeOptions));
    }

    public async Task<ConceptCode?> GetByCodeAsync(string code)
    {
        try
        {
            var filter = Builders<ConceptCode>.Filter.Eq(x => x.Code, code.ToUpper());
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting concept code by code: {code}", ex);
        }
    }

    public async Task<List<ConceptCode>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<PaginatedResult<ConceptCode>> GetAllAsync(PaginationConfigs paginationConfigs, FilterConceptCodeDTO filter)
    {
        var filterBuilder = Builders<ConceptCode>.Filter;
        var mongoFilter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            mongoFilter &= filterBuilder.Regex(x => x.Code,
                new MongoDB.Bson.BsonRegularExpression(filter.Search, "i"));
        }

        if (filter.IsFromSystem != null)
        {
            mongoFilter &= filterBuilder.Eq(x => x.IsFromSystem, filter.IsFromSystem.Value);
        }

        var totalCount = await _collection.CountDocumentsAsync(mongoFilter);

        var skip = (paginationConfigs.Page - 1) * paginationConfigs.PageSize;

        var deductionCodes = await _collection
            .Find(mongoFilter)
            .Skip(skip)
            .Limit(paginationConfigs.PageSize)
            .ToListAsync();

        return PaginatedResult<ConceptCode>.Create(deductionCodes, totalCount, paginationConfigs.Page, paginationConfigs.PageSize);
    }

    public async Task<ConceptCode> CreateAsync(ConceptCode deductionCode)
    {
        await _collection.InsertOneAsync(deductionCode);
        return deductionCode;
    }

    public async Task<ConceptCode> UpdateAsync(ConceptCode deductionCode)
    {
        var filter = Builders<ConceptCode>.Filter.Eq(x => x.Code, deductionCode.Code);
        await _collection.ReplaceOneAsync(filter, deductionCode);
        return deductionCode;
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        var filter = Builders<ConceptCode>.Filter.Regex(x => x.Code,
            new MongoDB.Bson.BsonRegularExpression(code, "i"));
        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<List<ConceptCode>> SearchByPatternAsync(string pattern)
    {
        var filter = Builders<ConceptCode>.Filter.Regex(x => x.Code,
            new MongoDB.Bson.BsonRegularExpression(pattern, "i"));
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<ConceptCode>> GetByPrefixAsync(string prefix)
    {
        var filter = Builders<ConceptCode>.Filter.Regex(x => x.Code,
            new MongoDB.Bson.BsonRegularExpression($"^{prefix.ToUpper()}", ""));
        return await _collection.Find(filter).ToListAsync();
    }
}