using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.Auth.DTOs.Inputs;

public record LogoutDTO
{
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("logout_all_devices")]
    public bool LogoutAllDevices { get; init; } = false;
}