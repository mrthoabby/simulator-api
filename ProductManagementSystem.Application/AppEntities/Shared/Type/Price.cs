using ProductManagementSystem.Application.AppEntities.Shared.Enum;

namespace ProductManagementSystem.Application.AppEntities.Shared.Type;

public class Price
{
    public decimal Amount { get; set; }
    public EnumCurrency Currency { get; set; }

    public Price() { }

    public Price(decimal amount, EnumCurrency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Price Create(decimal amount, EnumCurrency currency)
    {
        return new Price(amount, currency);
    }
}
