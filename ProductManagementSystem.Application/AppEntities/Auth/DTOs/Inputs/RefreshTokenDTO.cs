using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Auth.DTOs.Inputs;

public record RefreshTokenDTO
{
    [Required(ErrorMessage = "Refresh token is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Refresh token must be between 10 and 500 characters")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Audience específico para el nuevo token JWT.
    /// Debe coincidir con uno de los audiences configurados en JWT_AUDIENCE.
    /// Si no se especifica, se usa el primer audience de la lista.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; init; }
}