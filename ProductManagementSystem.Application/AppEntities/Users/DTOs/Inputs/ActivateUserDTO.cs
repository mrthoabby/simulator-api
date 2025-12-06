using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Users.DTOs.Inputs;

public readonly struct ActivateUserDTO
{
    [Required(ErrorMessage = "User name is required")]
    [StringLength(100, ErrorMessage = "User name cannot exceed 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("credentials")]
    [Required(ErrorMessage = "Credentials are required")]
    public CredentialsDTO Credentials { get; init; }

}