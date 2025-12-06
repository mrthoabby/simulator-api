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

    public Task<List<UserPlan>> GetAllWhereExistsAsync(string email)
    {
        return Task.FromResult(new List<UserPlan>());
    }

    public Task<List<UserPlan>> GetAllWhereIsMemberAsync(string email)
    {
        return Task.FromResult(new List<UserPlan>());
    }

    public Task<List<UserPlan>> GetAllWhereIsOwnerAsync(string email)
    {
        return Task.FromResult(new List<UserPlan>());
    }

    public Task<UserPlan> CreateAsync(UserPlan userPlan)
    {
        return Task.FromResult(userPlan);
    }

    public Task DeleteAsync(string id)
    {
        return Task.CompletedTask;
    }
}

