using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Users.Models;

namespace ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;

public struct UserDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("teams")]
    public List<CompanyInfoDTO>? Teams { get; set; }
}

public struct CompanyInfoDTO
{
    [JsonPropertyName("company_name")]
    public string Name { get; set; }
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; }
    [JsonPropertyName("subscription_name")]
    public string SubscriptionName { get; set; }
    [JsonPropertyName("subscription_id")]
    public string SubscriptionId { get; set; }

    [JsonPropertyName("user_plan_condition")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EnumUserPlanType UserPlanCondition { get; set; }
}