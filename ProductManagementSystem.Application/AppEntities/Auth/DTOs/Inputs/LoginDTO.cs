using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Auth.DTOs.Inputs;

public class LoginDTO
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// ID del dispositivo a revocar cuando se supere el límite de sesiones.
    /// Solo necesario en el segundo intento de login cuando ya se alcanzó el límite.
    /// </summary>
    [JsonPropertyName("device_id_to_revoke")]
    public string? DeviceIdToRevoke { get; set; }

    /// <summary>
    /// Audience específico para el token JWT.
    /// Debe coincidir con uno de los audiences configurados en JWT_AUDIENCE.
    /// Si no se especifica, se usa el primer audience de la lista.
    /// Ejemplos: "https://web.domain.com", "extension-key-123", "mobile-app"
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }
}