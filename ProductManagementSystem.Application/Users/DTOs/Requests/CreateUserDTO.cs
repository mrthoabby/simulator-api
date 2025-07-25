using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Users.Controllers.DTOs.Requests;

public record CreateUserDTO
{
    [Required(ErrorMessage = "User name is required")]
    [StringLength(100, ErrorMessage = "User name cannot exceed 100 characters")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [JsonPropertyName("password")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "Subscription ID is required")]
    [JsonPropertyName("subscription_id")]
    public required string SubscriptionId { get; init; }

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; init; }

}