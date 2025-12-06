using ProductManagementSystem.Application.AppEntities.UserPlans.DTOs.Outputs;

namespace ProductManagementSystem.Application.AppEntities.UserPlans.Services;

public interface IUserPlanService
{
    Task<PaymentProcessingResult> ProcessPendingPaymentsAsync();
    Task<int> SetOverDuePlansAsync();
    Task<int> InactivateOverduePlansAsync();
}
