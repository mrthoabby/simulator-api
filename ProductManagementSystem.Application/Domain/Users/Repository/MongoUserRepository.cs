using MongoDB.Driver;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Users.Models;

namespace ProductManagementSystem.Application.Domain.Users.Repository;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;


    public MongoUserRepository(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("users");
    }

    public async Task<User> CreateAsync(User user)
    {
        await _usersCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _usersCollection.Find(u => u.Credential!.Email == email).FirstOrDefaultAsync();
    }

    public async Task<PaginatedResult<User>> GetAllAsync(PaginationConfigs paginationConfigs, IUserFilters userFilters)
    {
        var filterDefinition = Builders<User>.Filter.Empty;
        var filterBuilder = Builders<User>.Filter;

        if (!string.IsNullOrEmpty(userFilters.Name))
            filterDefinition &= filterBuilder.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(userFilters.Name, "i"));

        if (!string.IsNullOrEmpty(userFilters.Email))
            filterDefinition &= filterBuilder.Regex(u => u.Credential!.Email, new MongoDB.Bson.BsonRegularExpression(userFilters.Email, "i"));


        var skip = (paginationConfigs.Page - 1) * paginationConfigs.PageSize;

        var totalCount = await _usersCollection.CountDocumentsAsync(filterDefinition);
        var totalPages = (int)Math.Ceiling((double)totalCount / paginationConfigs.PageSize);

        var users = await _usersCollection.Find(filterDefinition)
            .Skip(skip)
            .Limit(paginationConfigs.PageSize)
            .ToListAsync();

        return new PaginatedResult<User>
        {
            Items = users,
            TotalCount = (int)totalCount,
            Page = paginationConfigs.Page,
            PageSize = paginationConfigs.PageSize,
            TotalPages = totalPages,
            HasNextPage = paginationConfigs.Page < totalPages,
            HasPreviousPage = paginationConfigs.Page > 1
        };
    }

    public async Task<List<User>> GetAllNoPaginationAsync(IUserFilters userFilters)
    {
        var filterDefinition = Builders<User>.Filter.Empty;
        var filterBuilder = Builders<User>.Filter;

        if (!string.IsNullOrEmpty(userFilters.Name))
            filterDefinition &= filterBuilder.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(userFilters.Name, "i"));

        if (!string.IsNullOrEmpty(userFilters.Email))
            filterDefinition &= filterBuilder.Regex(u => u.Credential!.Email, new MongoDB.Bson.BsonRegularExpression(userFilters.Email, "i"));

        var users = await _usersCollection
            .Find(filterDefinition)
            .ToListAsync();

        return users;
    }

    public async Task DeleteAsync(string id)
    {
        var result = await _usersCollection.DeleteOneAsync(u => u.Id == id);

        if (result.DeletedCount == 0)
        {
            throw new ArgumentException("User not found");
        }
    }


}