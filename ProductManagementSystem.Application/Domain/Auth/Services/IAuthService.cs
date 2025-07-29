using ProductManagementSystem.Application.Domain.Auth.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Auth.DTOs.Outputs;

namespace ProductManagementSystem.Application.Domain.Auth.Services;

public interface IAuthService
{
    Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
    Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenDTO refreshTokenDto);
    Task LogoutAsync(LogoutDTO logoutDto, string? currentUserId = null);
    Task<bool> ValidateTokenAsync(string token);
    Task RevokeUserTokensAsync(string userId, string revokedBy);
    Task CleanupExpiredTokensAsync();
}