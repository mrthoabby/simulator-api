using Xunit;
using Moq;
using AutoMapper;
using ProductManagementSystem.Application.Domain.Auth.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Auth.Models;
using ProductManagementSystem.Application.Domain.Auth.Repository;
using ProductManagementSystem.Application.Domain.Auth.Enum;
using ProductManagementSystem.Application.Domain.Users.Models;
using ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Users.Repository;
using ProductManagementSystem.Application.Domain.UserPlans.Repository;
using ProductManagementSystem.Application.Domain.UserPlans.Models;
using ProductManagementSystem.Application.Domain.UserPlans.Domain;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;
using ProductManagementSystem.Application.Common;
using ProductManagementSystem.Application.Domain.Users.Services;
using ProductManagementSystem.Application.Common.Domain.Errors;
using ProductManagementSystem.Application.Domain.Shared.Type.Prices;

namespace ProductManagementSystem.Application.Domain.Auth.Services;

public class AuthServiceTests
{
    private readonly Mock<IAuthTokenRepository> _mockTokenRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserPlanRepository> _mockUserPlanRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IUserService> _mockUserService;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthSettings _authSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockTokenRepository = new Mock<IAuthTokenRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserPlanRepository = new Mock<IUserPlanRepository>();
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        // Setup default values
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-that-is-long-enough-for-jwt-validation-purposes",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };

        _authSettings = new AuthSettings
        {
            EnableAuth = true,
            RequireEmailConfirmation = false,
            MaxFailedAccessAttempts = 5,
            LockoutTimeInMinutes = 15
        };

        _securitySettings = new SecuritySettings
        {
            PasswordPepper = "test-pepper"
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["User-Agent"] = "Mozilla/5.0 (Test Browser)";
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _authService = new AuthService(
            _mockTokenRepository.Object,
            _mockUserRepository.Object,
            _mockUserPlanRepository.Object,
            _mockUserService.Object,
            _jwtSettings,
            _authSettings,
            _securitySettings,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockHttpContextAccessor.Object
        );
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        var user = CreateValidUser();
        var userPlans = new List<UserPlan> { CreateValidUserPlan() };
        var userDto = CreateValidUserDTO();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserPlanRepository.Setup(x => x.GetAllWhereExistsAsync(user.Credential.Email))
            .ReturnsAsync(userPlans);
        _mockTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(user.Id))
            .ReturnsAsync(new List<AuthToken>());
        _mockTokenRepository.Setup(x => x.CreateAsync(It.IsAny<AuthToken>()))
            .ReturnsAsync((AuthToken token) => token);

        // Mock UserMappingProfile.MapUserWithPlan statically - This would need to be handled differently
        // For now, we'll mock the mapper instead
        _mockMapper.Setup(x => x.Map<UserDTO>(user)).Returns(userDto);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.Equal("Bearer", result.AccessToken.Type);
        Assert.Equal("refresh", result.RefreshToken.Type);
        Assert.True(result.AccessToken.ExpiresAt > DateTime.UtcNow);
        Assert.True(result.RefreshToken.ExpiresAt > DateTime.UtcNow);

        _mockUserRepository.Verify(x => x.GetByEmailAsync(loginDto.Email), Times.Once);
        _mockTokenRepository.Verify(x => x.CreateAsync(It.IsAny<AuthToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "nonexistent@example.com",
            Password = "ValidPassword123!"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginAsync(loginDto));

        Assert.Equal("Invalid credentials", exception.Message);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(loginDto.Email), Times.Once);
        _mockTokenRepository.Verify(x => x.CreateAsync(It.IsAny<AuthToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = CreateValidUser();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginAsync(loginDto));

        Assert.Equal("Invalid credentials", exception.Message);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(loginDto.Email), Times.Once);
        _mockTokenRepository.Verify(x => x.CreateAsync(It.IsAny<AuthToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_DeviceLimitExceeded_ThrowsDeviceLimitExceededException()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        var user = CreateValidUser();
        var userPlans = new List<UserPlan> { CreateValidUserPlan(maxDevices: 1) };
        var activeTokens = new List<AuthToken>
        {
            AuthToken.Create(user.Id, "existing-token", TokenType.Refresh, DateTime.UtcNow.AddDays(1))
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserPlanRepository.Setup(x => x.GetAllWhereExistsAsync(user.Credential.Email))
            .ReturnsAsync(userPlans);
        _mockTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(user.Id))
            .ReturnsAsync(activeTokens);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DeviceLimitExceededException>(
            () => _authService.LoginAsync(loginDto));

        Assert.Equal(1, exception.MaxDevices);
        Assert.Single(exception.ActiveDevices);
        _mockTokenRepository.Verify(x => x.CreateAsync(It.IsAny<AuthToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_DeviceLimitExceededWithRevoke_RevokesTokenAndSucceeds()
    {
        // Arrange
        var tokenToRevoke = "token-to-revoke";
        var loginDto = new LoginDTO
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            DeviceIdToRevoke = tokenToRevoke
        };

        var user = CreateValidUser();
        var userPlans = new List<UserPlan> { CreateValidUserPlan(maxDevices: 1) };
        var tokenToRevokeObj = AuthToken.Create(user.Id, tokenToRevoke, TokenType.Refresh, DateTime.UtcNow.AddDays(1));
        var activeTokens = new List<AuthToken> { tokenToRevokeObj };
        var userDto = CreateValidUserDTO();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserPlanRepository.Setup(x => x.GetAllWhereExistsAsync(user.Credential.Email))
            .ReturnsAsync(userPlans);
        _mockTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(user.Id))
            .ReturnsAsync(activeTokens);
        _mockTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<AuthToken>()))
            .ReturnsAsync((AuthToken token) => token);
        _mockTokenRepository.Setup(x => x.CreateAsync(It.IsAny<AuthToken>()))
            .ReturnsAsync((AuthToken token) => token);
        _mockMapper.Setup(x => x.Map<UserDTO>(user)).Returns(userDto);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.True(tokenToRevokeObj.IsRevoked);
        _mockTokenRepository.Verify(x => x.UpdateAsync(It.Is<AuthToken>(t => t.IsRevoked)), Times.Once);
        _mockTokenRepository.Verify(x => x.CreateAsync(It.IsAny<AuthToken>()), Times.Exactly(2));
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDTO
        {
            RefreshToken = "valid-refresh-token"
        };

        var userId = "test-user-id";
        var user = CreateValidUser();
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        var userDto = CreateValidUserDTO();
        var userPlans = new List<UserPlan> { CreateValidUserPlan() };

        var refreshToken = AuthToken.Create(userId, refreshTokenDto.RefreshToken, TokenType.Refresh, DateTime.UtcNow.AddDays(1));

        _mockTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(refreshToken);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserPlanRepository.Setup(x => x.GetAllWhereExistsAsync(user.Credential.Email))
            .ReturnsAsync(userPlans);
        _mockTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<AuthToken>()))
            .ReturnsAsync((AuthToken token) => token);
        _mockTokenRepository.Setup(x => x.CreateAsync(It.IsAny<AuthToken>()))
            .ReturnsAsync((AuthToken token) => token);
        _mockMapper.Setup(x => x.Map<UserDTO>(user)).Returns(userDto);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        Assert.True(refreshToken.IsRevoked);
        _mockTokenRepository.Verify(x => x.UpdateAsync(It.Is<AuthToken>(t => t.IsRevoked)), Times.Once);
        _mockTokenRepository.Verify(x => x.CreateAsync(It.IsAny<AuthToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDTO
        {
            RefreshToken = "invalid-refresh-token"
        };

        _mockTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync((AuthToken?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.RefreshTokenAsync(refreshTokenDto));

        Assert.Equal("Invalid refresh token", exception.Message);
        _mockTokenRepository.Verify(x => x.CreateAsync(It.IsAny<AuthToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDTO
        {
            RefreshToken = "revoked-refresh-token"
        };

        var userId = "test-user-id";
        var revokedToken = AuthToken.Create(userId, refreshTokenDto.RefreshToken, TokenType.Refresh, DateTime.UtcNow.AddDays(1));

        // Revoke the token to make it invalid
        revokedToken.Revoke("Test revocation");

        _mockTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(revokedToken);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.RefreshTokenAsync(refreshTokenDto));

        Assert.Equal("Invalid refresh token", exception.Message);
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_SingleDevice_RevokesTokenSuccessfully()
    {
        // Arrange
        var logoutDto = new LogoutDTO
        {
            RefreshToken = "valid-refresh-token",
            LogoutAllDevices = false
        };

        var userId = "test-user-id";
        var refreshToken = AuthToken.Create(userId, logoutDto.RefreshToken, TokenType.Refresh, DateTime.UtcNow.AddDays(1));

        _mockTokenRepository.Setup(x => x.GetByTokenAsync(logoutDto.RefreshToken))
            .ReturnsAsync(refreshToken);
        _mockTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<AuthToken>()))
            .ReturnsAsync((AuthToken token) => token);

        // Act
        await _authService.LogoutAsync(logoutDto, userId);

        // Assert
        Assert.True(refreshToken.IsRevoked);
        _mockTokenRepository.Verify(x => x.UpdateAsync(It.Is<AuthToken>(t => t.IsRevoked)), Times.Once);
        _mockTokenRepository.Verify(x => x.GetActiveTokensByUserIdAsync(userId), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_AllDevices_RevokesAllUserTokens()
    {
        // Arrange
        var logoutDto = new LogoutDTO
        {
            RefreshToken = "valid-refresh-token",
            LogoutAllDevices = true
        };

        var userId = "test-user-id";

        _mockTokenRepository.Setup(x => x.RevokeAllTokensByUserIdAsync(userId, "User logout all devices"))
            .Returns(Task.CompletedTask);

        // Act
        await _authService.LogoutAsync(logoutDto, userId);

        // Assert
        _mockTokenRepository.Verify(x => x.RevokeAllTokensByUserIdAsync(userId, "User logout all devices"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_InvalidToken_DoesNotThrow()
    {
        // Arrange
        var logoutDto = new LogoutDTO
        {
            RefreshToken = "invalid-refresh-token",
            LogoutAllDevices = false
        };

        _mockTokenRepository.Setup(x => x.GetByTokenAsync(logoutDto.RefreshToken))
            .ReturnsAsync((AuthToken?)null);

        // Act - Should not throw an exception
        await _authService.LogoutAsync(logoutDto, "test-user-id");

        // Assert - Verify the token lookup was attempted
        _mockTokenRepository.Verify(x => x.GetByTokenAsync(logoutDto.RefreshToken), Times.Once);
    }

    #endregion

    #region Helper Methods

    private User CreateValidUser()
    {
        var credential = Credential.Create("test@example.com", "ValidPassword123!", "test-pepper");
        return User.Create("Test User", credential);
    }

    private UserPlan CreateValidUserPlan(int maxDevices = 5)
    {
        var restrictions = Restrictions.CreateBuilder()
            .SetMaxProducts(100)
            .SetMaxUsers(10)
            .SetMaxCompetitors(50)
            .SetMaxCustomDeductions(20)
            .SetMaxSimulations(1000)
            .SetMaxActiveSessionDevices(maxDevices)
            .EnablePDFExport()
            .EnableSimulationComparison()
            .EnableExcelExport()
            .Build();

        var price = Price.Create(29.99m, EnumCurrency.USD);

        var subscription = Subscription.Create(
            "Test Plan",
            "Test subscription with all features",
            price,
            ProductManagementSystem.Application.Domain.Subscriptions.Enums.EnumSubscriptionPeriod.Monthly,
            restrictions
        );

        var company = Company.Create("Test Company");
        return UserPlan.Create(subscription, company, "test@example.com");
    }

    private UserDTO CreateValidUserDTO()
    {
        return new UserDTO
        {
            Id = "test-user-id",
            Name = "Test User",
            Email = "test@example.com",
            Teams = new List<ProductManagementSystem.Application.Domain.Users.DTOs.Outputs.CompanyInfoDTO>
            {
                new ProductManagementSystem.Application.Domain.Users.DTOs.Outputs.CompanyInfoDTO
                {
                    Name = "Test Company",
                    CompanyId = "company-123",
                    SubscriptionName = "Test Plan",
                    SubscriptionId = "subscription-123",
                    UserPlanCondition = ProductManagementSystem.Application.Domain.Users.Models.EnumUserPlanType.Owner
                }
            }
        };
    }

    #endregion
}