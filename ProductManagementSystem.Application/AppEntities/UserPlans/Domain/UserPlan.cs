using ProductManagementSystem.Application.AppEntities.Subscriptions.Models;
using ProductManagementSystem.Application.AppEntities.Users.Models;

namespace ProductManagementSystem.Application.AppEntities.UserPlans.Domain;

public class UserPlan
{
    public string Id { get; set; } = default!;
    public string OwnerEmail { get; set; } = default!;
    public Subscription Subscription { get; set; } = default!;
    public Company Company { get; set; } = default!;
    public bool IsActive { get; set; }

    public static UserPlan Create(Subscription subscription, Company company, string email)
    {
        return new UserPlan
        {
            Id = Guid.NewGuid().ToString(),
            Subscription = subscription,
            Company = company,
            OwnerEmail = email,
            IsActive = true
        };
    }
}
