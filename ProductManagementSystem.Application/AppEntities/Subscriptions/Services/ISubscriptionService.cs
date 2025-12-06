using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Outputs;

namespace ProductManagementSystem.Application.AppEntities.Subscriptions.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDTO> CreateAsync(CreateSubscriptionDTO subscription);
    Task<SubscriptionDTO?> GetByIdAsync(string id);
    Task<SubscriptionDTO?> GetByNameAsync(string name);
    Task<PaginatedResult<SubscriptionDTO>> GetAllAsync(SubscriptionFilterDTO filter);
    Task DeleteAsync(string id);
}