using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Users.DTOs.Inputs;

public readonly struct CreateUserDTO
{
    [Required(ErrorMessage = "User name is required")]
    [StringLength(100, ErrorMessage = "User name cannot exceed 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("credentials")]
    [Required(ErrorMessage = "Credentials are required")]
    public CredentialsDTO Credentials { get; init; }

    [Required(ErrorMessage = "Subscription ID is required")]
    [JsonPropertyName("subscription_id")]
    public string SubscriptionId { get; init; }

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; init; }

    [JsonPropertyName("use_trial")]
    public bool UseTrial { get; init; }

}

public struct CredentialsDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [JsonPropertyName("email")]
    public string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [JsonPropertyName("password")]
    public string Password { get; init; }
}