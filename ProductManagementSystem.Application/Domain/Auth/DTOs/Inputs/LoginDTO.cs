using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.Auth.DTOs.Inputs;

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
}