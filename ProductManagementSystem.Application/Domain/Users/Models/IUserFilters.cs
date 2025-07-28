namespace ProductManagementSystem.Application.Domain.Users.Models;

public interface IUserFilters
{
    string? Name { get; set; }
    string? Email { get; set; }
    string? CompanyId { get; set; }
    string? SubscriptionId { get; set; }
    bool? IsActive { get; set; }
    bool WithUserPlans { get; set; }
}