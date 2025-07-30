namespace ProductManagementSystem.Infrastructure.Providers;

public interface IPaymentProvider
{
    Task<bool> ProcessPayment(string processId);
    Task<bool> ReversePayment(string processId);
}