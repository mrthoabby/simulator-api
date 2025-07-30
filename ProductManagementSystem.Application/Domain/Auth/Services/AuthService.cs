using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProductManagementSystem.Application.Domain.Auth.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Auth.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Auth.Models;
using ProductManagementSystem.Application.Domain.Auth.Repository;
using ProductManagementSystem.Application.Domain.Users.Services;
using ProductManagementSystem.Application.Common;

using ProductManagementSystem.Application.Common.Domain.Errors;
using AutoMapper;
using ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Auth.Enum;
using ProductManagementSystem.Application.Domain.Users.Repository;
using ProductManagementSystem.Application.Domain.Users.Models;
using ProductManagementSystem.Application.Domain.Users.Mappings;
using ProductManagementSystem.Application.Common.Helpers;
using ProductManagementSystem.Application.Domain.UserPlans.Repository;


namespace ProductManagementSystem.Application.Domain.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAuthTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserPlanRepository _userPlanRepository;
    private readonly IUserService _userService;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthSettings _authSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IAuthTokenRepository tokenRepository,
        IUserRepository userRepository,
        IUserPlanRepository userPlanRepository,
        IUserService userService,
        JwtSettings jwtSettings,
        AuthSettings authSettings,
        SecuritySettings securitySettings,
        IMapper mapper,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _userPlanRepository = userPlanRepository;
        _userService = userService;
        _jwtSettings = jwtSettings;
        _authSettings = authSettings;
        _securitySettings = securitySettings;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
    {

        _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", loginDto.Email);
            throw new UnauthorizedException(AuthServiceValues.Errors.InvalidCredentials);
        }

        var isCredentialValid = user.Credential.VerifyPassword(loginDto.Password, _securitySettings.PasswordPepper);
        if (!isCredentialValid)
        {
            _logger.LogWarning("Invalid credentials for email: {Email}", loginDto.Email);
            throw new UnauthorizedException(AuthServiceValues.Errors.InvalidCredentials);
        }

        await HandleSessionLimitsAsync(user, loginDto.DeviceIdToRevoke);

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        var accessAuthToken = AuthToken.Create(user.Id, accessToken, TokenType.Access, accessTokenExpiry);
        var refreshAuthToken = AuthToken.Create(user.Id, refreshToken, TokenType.Refresh, refreshTokenExpiry);

        await TransactionHelper.CreateParallelVoidAsync(
            firstFlow: async () => await _tokenRepository.CreateAsync(accessAuthToken),
            secondFlow: async () => await _tokenRepository.CreateAsync(refreshAuthToken),
            deleteFirst: async (accessToken) => await _tokenRepository.DeleteAsync(accessToken.Id),
            deleteSecond: async (refreshToken) => await _tokenRepository.DeleteAsync(refreshToken.Id),
            firstStepName: "Create Access Token",
            secondStepName: "Create Refresh Token",
            logger: _logger
        );

        _logger.LogInformation("Login successful for user {UserId}", user.Id);

        var userPlans = await _userPlanRepository.GetAllWhereExistsAsync(user.Credential.Email);

        return new AuthResponseDTO
        {
            User = UserMappingProfile.MapUserWithPlan(user, userPlans),
            AccessToken = new TokenDTO
            {
                Token = accessToken,
                Type = "Bearer",
                ExpiresAt = accessTokenExpiry,
                CreatedAt = DateTime.UtcNow
            },
            RefreshToken = new TokenDTO
            {
                Token = refreshToken,
                Type = "refresh",
                ExpiresAt = refreshTokenExpiry,
                CreatedAt = DateTime.UtcNow
            },
            AuthenticatedAt = DateTime.UtcNow
        };
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenDTO refreshTokenDto)
    {
        if (!_authSettings.EnableAuth)
        {
            throw new UnauthorizedException("Authentication is currently disabled");
        }

        _logger.LogInformation("Refresh token attempt");

        var refreshAuthToken = await _tokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);
        if (refreshAuthToken == null || !refreshAuthToken.IsValid || refreshAuthToken.TokenType != TokenType.Refresh)
        {
            _logger.LogWarning("Invalid refresh token provided");
            throw new UnauthorizedException(AuthServiceValues.Errors.InvalidRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(refreshAuthToken.UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found for refresh token. UserId: {UserId}", refreshAuthToken.UserId);
            throw new UnauthorizedException(AuthServiceValues.Errors.InvalidRefreshToken);
        }

        refreshAuthToken.Revoke("Token refreshed");
        await _tokenRepository.UpdateAsync(refreshAuthToken);

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        var newAccessAuthToken = AuthToken.Create(user.Id, newAccessToken, TokenType.Access, accessTokenExpiry);
        var newRefreshAuthToken = AuthToken.Create(user.Id, newRefreshToken, TokenType.Refresh, refreshTokenExpiry);

        await TransactionHelper.CreateParallelVoidAsync(
            firstFlow: async () => await _tokenRepository.CreateAsync(newAccessAuthToken),
            secondFlow: async () => await _tokenRepository.CreateAsync(newRefreshAuthToken),
            deleteFirst: async (accessToken) => await _tokenRepository.DeleteAsync(accessToken.Id),
            deleteSecond: async (refreshToken) => await _tokenRepository.DeleteAsync(refreshToken.Id),
            firstStepName: "Create New Access Token",
            secondStepName: "Create New Refresh Token",
            logger: _logger
        );

        _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

        return new AuthResponseDTO
        {
            User = _mapper.Map<UserDTO>(user),
            AccessToken = new TokenDTO
            {
                Token = newAccessToken,
                Type = "Bearer",
                ExpiresAt = accessTokenExpiry,
                CreatedAt = DateTime.UtcNow
            },
            RefreshToken = new TokenDTO
            {
                Token = newRefreshToken,
                Type = "refresh",
                ExpiresAt = refreshTokenExpiry,
                CreatedAt = DateTime.UtcNow
            },
            AuthenticatedAt = DateTime.UtcNow
        };
    }

    public async Task LogoutAsync(LogoutDTO logoutDto, string? currentUserId = null)
    {
        _logger.LogInformation("Logout attempt for user {UserId}", currentUserId);

        if (logoutDto.LogoutAllDevices && !string.IsNullOrEmpty(currentUserId))
        {
            await _tokenRepository.RevokeAllTokensByUserIdAsync(currentUserId, "User logout all devices");
            _logger.LogInformation("All tokens revoked for user {UserId}", currentUserId);
        }
        else if (!string.IsNullOrEmpty(logoutDto.RefreshToken))
        {
            var refreshToken = await _tokenRepository.GetByTokenAsync(logoutDto.RefreshToken);
            if (refreshToken != null && refreshToken.IsValid)
            {
                refreshToken.Revoke("User logout");
                await _tokenRepository.UpdateAsync(refreshToken);
                _logger.LogInformation("Refresh token revoked for user {UserId}", refreshToken.UserId);
            }
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var authToken = await _tokenRepository.GetByTokenAsync(token);
            return authToken != null && authToken.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed for token: {Token}", token[..Math.Min(token.Length, 10)] + "...");
            return false;
        }
    }

    public async Task RevokeUserTokensAsync(string userId, string revokedBy)
    {
        var adminDefined = false;
        if (!adminDefined) throw new UnauthorizedException("JUST ADMIN");

        if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        if (string.IsNullOrEmpty(revokedBy)) throw new ArgumentException("Revoked by cannot be null or empty", nameof(revokedBy));

        await _tokenRepository.RevokeAllTokensByUserIdAsync(userId, revokedBy);
        _logger.LogInformation("All tokens revoked for user {UserId} by {RevokedBy}", userId, revokedBy);
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var adminDefined = false;
        if (!adminDefined) throw new UnauthorizedException("JUST ADMIN");
        await _tokenRepository.RevokeExpiredTokensAsync();
        _logger.LogInformation("Expired tokens cleanup completed");
    }

    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Credential.Email),
            new(ClaimTypes.Name, user.Name),
            new(AuthServiceValues.Values.TokenTypeProperty, TokenType.Access.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }



    private async Task HandleSessionLimitsAsync(User user, string? includedDeviceIdToRevoke)
    {
        var userPlans = await _userPlanRepository.GetAllWhereExistsAsync(user.Credential.Email);

        if (!userPlans.Any() || !userPlans.Any(p => p.IsActive))
        {
            _logger.LogWarning("No active plans found for user: {Email}", user.Credential.Email);
            throw new InvalidOperationException("No active subscription found");
        }

        var maxDevices = userPlans
            .Where(p => p.IsActive)
            .Max(p => p.Subscription.Restrictions.MaxActiveSessionDevices);

        if (maxDevices <= 0)
        {
            _logger.LogInformation("No session limit configured for user: {Email}", user.Credential.Email);
            return;
        }

        var activeTokens = await _tokenRepository.GetActiveTokensByUserIdAsync(user.Id);

        _logger.LogInformation("User {UserId} has {ActiveCount} active sessions, limit is {MaxLimit}",
            user.Id, activeTokens.Count, maxDevices);

        if (activeTokens.Count >= maxDevices)
        {
            if (string.IsNullOrEmpty(includedDeviceIdToRevoke))
            {
                var userAgent = GetUserAgentFromRequest();
                var devices = activeTokens.Select(token => new DeviceInfoDTO
                {
                    Id = token.Token,
                    DeviceName = DeviceDetectionHelper.ExtractDeviceName(userAgent),
                    LoginDate = token.CreatedAt,
                    LastActivity = token.CreatedAt
                }).OrderByDescending(d => d.LastActivity).ToList();

                _logger.LogWarning("Device limit exceeded for user {UserId}. Showing {DeviceCount} active devices",
                    user.Id, devices.Count);

                throw new DeviceLimitExceededException(maxDevices, devices);
            }
            else
            {
                var tokenToRevoke = activeTokens.FirstOrDefault(t => t.Token == includedDeviceIdToRevoke);
                if (tokenToRevoke != null)
                {
                    tokenToRevoke.Revoke("User selected to close this session for new device login");
                    await _tokenRepository.UpdateAsync(tokenToRevoke);

                    _logger.LogInformation("Revoked device session {TokenId} for user {UserId} by user selection",
                        tokenToRevoke.Id, user.Id);
                }
                else
                {
                    _logger.LogWarning("Device ID to revoke not found: {DeviceId} for user {UserId}",
                        includedDeviceIdToRevoke, user.Id);
                    throw new BadRequestException("Invalid device ID provided");
                }
            }
        }
    }

    private string? GetUserAgentFromRequest()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get User-Agent from request");
            return null;
        }
    }
}

public static class AuthServiceValues
{
    public static class Errors
    {
        public const string InvalidCredentials = "Invalid credentials";
        public const string InvalidRefreshToken = "Invalid refresh token";
    }

    public static class Messages
    {
        public const string LoginSuccessful = "Login successful";
    }

    public static class Values
    {
        public const int MaxLoginAttempts = 3;
        public const string TokenTypeProperty = "token_type";
    }
}