using ProductManagementSystem.Application.Domain.Subscriptions.Models;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Requests;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Repository;

public interface ISubscriptionRepository
{
    Task<Subscription> CreateAsync(Subscription subscription);
    Task<Subscription?> GetByIdAsync(string id);
    Task<Subscription?> GetByNameAsync(string name);
    Task<PaginatedResult<Subscription>> GetAllAsync(SubscriptionFilterDTO filter);
    Task DeleteAsync(string id);
}
