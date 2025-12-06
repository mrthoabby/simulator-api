using ProductManagementSystem.Application.AppEntities.Subscriptions.Models;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Inputs;

namespace ProductManagementSystem.Application.AppEntities.Subscriptions.Repository;

public interface ISubscriptionRepository
{
    Task<Subscription> CreateAsync(Subscription subscription);
    Task<Subscription?> GetByIdAsync(string id);
    Task<Subscription?> GetByNameAsync(string name);
    Task<PaginatedResult<Subscription>> GetAllAsync(SubscriptionFilterDTO filter);
    Task DeleteAsync(string id);
}
