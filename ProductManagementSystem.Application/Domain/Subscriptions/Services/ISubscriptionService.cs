using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Requests;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Responses;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDTO> CreateAsync(CreateSubscriptionDTO subscription);
    Task<SubscriptionDTO?> GetByIdAsync(string id);
    Task<SubscriptionDTO?> GetByNameAsync(string name);
    Task<PaginatedResult<SubscriptionDTO>> GetAllAsync(SubscriptionFilterDTO filter);
    Task DeleteAsync(string id);
}