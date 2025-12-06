using ProductManagementSystem.Application.AppEntities.UserPlans.Domain;

namespace ProductManagementSystem.Application.AppEntities.UserPlans.Repository;

public interface IUserPlanRepository
{
    Task<List<UserPlan>> GetAllWhereExistsAsync(string email);
    Task<List<UserPlan>> GetAllWhereIsMemberAsync(string email);
    Task<List<UserPlan>> GetAllWhereIsOwnerAsync(string email);
    Task<UserPlan> CreateAsync(UserPlan userPlan);
    Task DeleteAsync(string id);
}
