using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Users.DTOs.Outputs;

namespace ProductManagementSystem.Application.AppEntities.Auth.DTOs.Outputs;

public struct AuthResponseDTO
{
    [JsonPropertyName("user")]
    public UserDTO? User { get; init; }

    [JsonPropertyName("access_token")]
    public TokenDTO AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public TokenDTO RefreshToken { get; init; }

    [JsonPropertyName("authenticated_at")]
    public DateTime AuthenticatedAt { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn => (int)(AccessToken.ExpiresAt - DateTime.UtcNow).TotalSeconds;
}