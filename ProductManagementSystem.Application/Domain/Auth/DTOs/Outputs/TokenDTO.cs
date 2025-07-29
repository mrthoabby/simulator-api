using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.Auth.DTOs.Outputs;

public struct TokenDTO
{
    [JsonPropertyName("token")]
    public string Token { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("is_expired")]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}