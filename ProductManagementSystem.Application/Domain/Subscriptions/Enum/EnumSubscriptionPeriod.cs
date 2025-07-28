namespace ProductManagementSystem.Application.Domain.Subscriptions.Enums;

/// <summary>
/// Subscription period enumeration with values representing days
/// </summary>
public enum EnumSubscriptionPeriod
{
    /// <summary>
    /// Monthly subscription (30 days)
    /// </summary>
    Monthly = 30,

    /// <summary>
    /// Quarterly subscription (90 days)
    /// </summary>
    Quarterly = 90,

    /// <summary>
    /// Yearly subscription (365 days)
    /// </summary>
    Yearly = 365
}