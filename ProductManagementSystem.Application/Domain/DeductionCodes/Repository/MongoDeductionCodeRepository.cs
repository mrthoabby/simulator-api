using MongoDB.Driver;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.Models;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.Repository;

public class MongoDeductionCodeRepository : IDeductionCodeRepository
{
    private readonly IMongoCollection<DeductionCode> _collection;
    private const string CollectionName = "deduction_codes";

    public MongoDeductionCodeRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<DeductionCode>(CollectionName);
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var codeIndex = Builders<DeductionCode>.IndexKeys.Ascending(x => x.Code);
        var codeOptions = new CreateIndexOptions { Unique = true };
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<DeductionCode>(codeIndex, codeOptions));
    }

    public async Task<DeductionCode?> GetByIdAsync(string id)
    {
        var filter = Builders<DeductionCode>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<DeductionCode?> GetByCodeAsync(string code)
    {
        var filter = Builders<DeductionCode>.Filter.Eq(x => x.Code, code.ToUpper());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<DeductionCode>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<PaginatedResult<DeductionCode>> GetFilteredAsync(PaginationConfigs paginationConfigs, FilterDeductionCodeDTO filter)
    {
        var filterBuilder = Builders<DeductionCode>.Filter;
        var mongoFilter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(filter.Code))
        {
            mongoFilter &= filterBuilder.Regex(x => x.Code,
                new MongoDB.Bson.BsonRegularExpression(filter.Code, "i"));
        }

        if (!string.IsNullOrWhiteSpace(filter.Pattern))
        {
            mongoFilter &= filterBuilder.Regex(x => x.Code,
                new MongoDB.Bson.BsonRegularExpression(filter.Pattern, "i"));
        }

        var totalCount = await _collection.CountDocumentsAsync(mongoFilter);

        var page = paginationConfigs.Page;
        var pageSize = paginationConfigs.PageSize;

        var deductionCodes = await _collection
            .Find(mongoFilter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PaginatedResult<DeductionCode>
        {
            Items = deductionCodes,
            TotalCount = (int)totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<DeductionCode> CreateAsync(DeductionCode deductionCode)
    {
        await _collection.InsertOneAsync(deductionCode);
        return deductionCode;
    }

    public async Task<DeductionCode> UpdateAsync(DeductionCode deductionCode)
    {
        var filter = Builders<DeductionCode>.Filter.Eq(x => x.Code, deductionCode.Code);
        await _collection.ReplaceOneAsync(filter, deductionCode);
        return deductionCode;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<DeductionCode>.Filter.Eq("_id", id);
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        var filter = Builders<DeductionCode>.Filter.Eq(x => x.Code, code.ToUpper());
        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<List<DeductionCode>> SearchByPatternAsync(string pattern)
    {
        var filter = Builders<DeductionCode>.Filter.Regex(x => x.Code,
            new MongoDB.Bson.BsonRegularExpression(pattern, "i"));
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<DeductionCode>> GetByPrefixAsync(string prefix)
    {
        var filter = Builders<DeductionCode>.Filter.Regex(x => x.Code,
            new MongoDB.Bson.BsonRegularExpression($"^{prefix.ToUpper()}", ""));
        return await _collection.Find(filter).ToListAsync();
    }
}