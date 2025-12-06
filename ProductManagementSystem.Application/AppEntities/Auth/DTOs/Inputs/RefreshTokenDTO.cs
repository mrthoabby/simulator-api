using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Auth.DTOs.Inputs;

public record RefreshTokenDTO
{
    [Required(ErrorMessage = "Refresh token is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Refresh token must be between 10 and 500 characters")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;
}