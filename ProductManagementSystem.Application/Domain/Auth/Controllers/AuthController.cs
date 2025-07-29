using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductManagementSystem.Application.Domain.Auth.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Auth.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Auth.Services;
using ProductManagementSystem.Application.Common.Domain.Errors;
using System.Security.Claims;

namespace ProductManagementSystem.Application.Domain.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO request)
    {
        _logger.LogInformation("Login request received for email: {Email}", request.Email);

        var authResponse = await _authService.LoginAsync(request);

        _logger.LogInformation("Login successful for email: {Email}", request.Email);
        return Ok(authResponse);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponseDTO>> RefreshToken([FromBody] RefreshTokenDTO request)
    {
        _logger.LogInformation("Token refresh request received");

        var authResponse = await _authService.RefreshTokenAsync(request);

        _logger.LogInformation("Token refresh successful");
        return Ok(authResponse);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] LogoutDTO request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation("Logout request received for user: {UserId}", currentUserId);

        await _authService.LogoutAsync(request, currentUserId);

        _logger.LogInformation("Logout successful for user: {UserId}", currentUserId);
        return NoContent();
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> ValidateToken([FromBody] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { valid = false, message = "Token is required" });
        }

        var isValid = await _authService.ValidateTokenAsync(token);

        return Ok(new { valid = isValid });
    }

    [HttpPost("revoke/{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeUserTokens(string userId)
    {
        var currentAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentAdminName = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required");
        }

        _logger.LogInformation("Admin {AdminId} revoking all tokens for user {UserId}", currentAdminId, userId);

        await _authService.RevokeUserTokensAsync(userId, $"Admin: {currentAdminName} ({currentAdminId})");

        _logger.LogInformation("All tokens revoked for user {UserId} by admin {AdminId}", userId, currentAdminId);
        return NoContent();
    }

    [HttpPost("cleanup")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CleanupExpiredTokens()
    {
        var currentAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation("Admin {AdminId} initiated token cleanup", currentAdminId);

        await _authService.CleanupExpiredTokensAsync();

        _logger.LogInformation("Token cleanup completed by admin {AdminId}", currentAdminId);
        return NoContent();
    }
}