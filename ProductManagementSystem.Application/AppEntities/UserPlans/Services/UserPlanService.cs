using ProductManagementSystem.Application.AppEntities.UserPlans.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.UserPlans.Repository;

namespace ProductManagementSystem.Application.AppEntities.UserPlans.Services;

public class UserPlanService : IUserPlanService
{
    private readonly IUserPlanRepository _repository;

    public UserPlanService(IUserPlanRepository repository)
    {
        _repository = repository;
    }

    public Task<PaymentProcessingResult> ProcessPendingPaymentsAsync()
    {
        return Task.FromResult(new PaymentProcessingResult { SussesPayments = 0, FailedPayments = 0 });
    }

    public Task<int> SetOverDuePlansAsync()
    {
        return Task.FromResult(0);
    }

    public Task<int> InactivateOverduePlansAsync()
    {
        return Task.FromResult(0);
    }
}

