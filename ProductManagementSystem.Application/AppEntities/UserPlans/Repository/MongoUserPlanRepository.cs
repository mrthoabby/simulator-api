using MongoDB.Driver;
using ProductManagementSystem.Application.AppEntities.UserPlans.Domain;

namespace ProductManagementSystem.Application.AppEntities.UserPlans.Repository;

public class MongoUserPlanRepository : IUserPlanRepository
{
    private readonly IMongoCollection<UserPlan> _collection;

    public MongoUserPlanRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<UserPlan>("UserPlans");
    }

    public async Task<List<UserPlan>> GetAllWhereExistsAsync(string email)
    {
        var filter = Builders<UserPlan>.Filter.Eq(x => x.OwnerEmail, email);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<UserPlan>> GetAllWhereIsMemberAsync(string email)
    {
        var filter = Builders<UserPlan>.Filter.Eq(x => x.OwnerEmail, email);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<UserPlan>> GetAllWhereIsOwnerAsync(string email)
    {
        var filter = Builders<UserPlan>.Filter.Eq(x => x.OwnerEmail, email);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<UserPlan> CreateAsync(UserPlan userPlan)
    {
        await _collection.InsertOneAsync(userPlan);
        return userPlan;
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<UserPlan>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }
}

