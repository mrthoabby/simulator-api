namespace ProductManagementSystem.Infrastructure.Providers;

public class MockPaymentProvider : IPaymentProvider
{
    public Task<bool> ProcessPayment(string processId)
    {
        return Task.FromResult(true);
    }

    public Task<bool> ReversePayment(string processId)
    {
        return Task.FromResult(true);
    }
}
