using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Outputs;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDTO> CreateAsync(CreateSubscriptionDTO subscription);
    Task<SubscriptionDTO?> GetByIdAsync(string id);
    Task<SubscriptionDTO?> GetByNameAsync(string name);
    Task<PaginatedResult<SubscriptionDTO>> GetAllAsync(SubscriptionFilterDTO filter);
    Task DeleteAsync(string id);
}