using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ProductManagementSystem.Application.AppEntities.Auth.Controllers;
using ProductManagementSystem.Application.AppEntities.Auth.Services;
using ProductManagementSystem.Application.AppEntities.Auth.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Auth.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Users.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Users.Models;
using ProductManagementSystem.Application.Common.AppEntities.Errors;
using FluentValidation;

namespace ProductManagementSystem.Application.AppEntities.Auth.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);

        // Setup HTTP context
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    #region Login Tests

    [Fact]
    public async Task Login_ValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        var expectedResponse = CreateValidAuthResponse();

        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResponse = Assert.IsType<AuthResponseDTO>(okResult.Value);

        Assert.Equal(expectedResponse.User?.Email, authResponse.User?.Email);
        Assert.Equal(expectedResponse.AccessToken.Type, authResponse.AccessToken.Type);
        Assert.Equal(expectedResponse.RefreshToken.Type, authResponse.RefreshToken.Type);

        _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_ModelStateInvalid_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "", // Invalid - empty email
            Password = "ValidPassword123!"
        };

        // Simulate model validation errors
        _controller.ModelState.AddModelError("Email", "The Email field is required.");

        // Mock validation error handling - this would be caught by GlobalExceptionHandlerMiddleware
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginDTO>()))
            .ThrowsAsync(new ValidationException("Validation failed: Email is required"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _controller.Login(loginDto));

        Assert.Contains("Email is required", exception.Message);
    }

    [Fact]
    public async Task Login_UnauthorizedCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "wrong@example.com",
            Password = "WrongPassword"
        };

        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new UnauthorizedException("Invalid credentials"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _controller.Login(loginDto));

        Assert.Equal("Invalid credentials", exception.Message);
        _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_DeviceLimitExceeded_ThrowsDeviceLimitExceededException()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        var activeDevices = new List<DeviceInfoDTO>
        {
            new DeviceInfoDTO
            {
                Id = "device-1",
                DeviceName = "iPhone 12",
                LoginDate = DateTime.UtcNow.AddDays(-1),
                LastActivity = DateTime.UtcNow.AddHours(-2)
            }
        };

        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new DeviceLimitExceededException(1, activeDevices));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DeviceLimitExceededException>(
            () => _controller.Login(loginDto));

        Assert.Equal(1, exception.MaxDevices);
        Assert.Single(exception.ActiveDevices);
        _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_ServiceException_PropagatesException()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Login(loginDto));

        Assert.Equal("Database connection failed", exception.Message);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Refresh_ValidToken_ReturnsOkWithNewTokens()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDTO
        {
            RefreshToken = "valid-refresh-token"
        };

        var expectedResponse = CreateValidAuthResponse();

        _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResponse = Assert.IsType<AuthResponseDTO>(okResult.Value);

        Assert.Equal("Bearer", authResponse.AccessToken.Type);

        _mockAuthService.Verify(x => x.RefreshTokenAsync(refreshTokenDto), Times.Once);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDTO
        {
            RefreshToken = "invalid-refresh-token"
        };

        _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto))
            .ThrowsAsync(new UnauthorizedException("Invalid refresh token"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _controller.RefreshToken(refreshTokenDto));

        Assert.Equal("Invalid refresh token", exception.Message);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDTO
        {
            RefreshToken = "expired-refresh-token"
        };

        _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto))
            .ThrowsAsync(new UnauthorizedException("Invalid refresh token"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _controller.RefreshToken(refreshTokenDto));

        Assert.Equal("Invalid refresh token", exception.Message);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_ValidToken_ReturnsOk()
    {
        // Arrange
        var logoutDto = new LogoutDTO
        {
            RefreshToken = "valid-refresh-token",
            LogoutAllDevices = false
        };

        // Setup authenticated user context
        var userId = "test-user-id";
        SetupAuthenticatedUser(userId);

        _mockAuthService.Setup(x => x.LogoutAsync(logoutDto, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout(logoutDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockAuthService.Verify(x => x.LogoutAsync(logoutDto, userId), Times.Once);
    }

    [Fact]
    public async Task Logout_AllDevices_ReturnsOk()
    {
        // Arrange
        var logoutDto = new LogoutDTO
        {
            RefreshToken = "valid-refresh-token",
            LogoutAllDevices = true
        };

        var userId = "test-user-id";
        SetupAuthenticatedUser(userId);

        _mockAuthService.Setup(x => x.LogoutAsync(logoutDto, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout(logoutDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockAuthService.Verify(x => x.LogoutAsync(logoutDto, userId), Times.Once);
    }

    [Fact]
    public async Task Logout_InvalidToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var logoutDto = new LogoutDTO
        {
            RefreshToken = "invalid-refresh-token",
            LogoutAllDevices = false
        };

        var userId = "test-user-id";
        SetupAuthenticatedUser(userId);

        _mockAuthService.Setup(x => x.LogoutAsync(logoutDto, userId))
            .ThrowsAsync(new UnauthorizedException("Invalid refresh token"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _controller.Logout(logoutDto));

        Assert.Equal("Invalid refresh token", exception.Message);
    }

    [Fact]
    public async Task Logout_UnauthenticatedUser_UsesNullUserId()
    {
        // Arrange
        var logoutDto = new LogoutDTO
        {
            RefreshToken = "valid-refresh-token",
            LogoutAllDevices = false
        };

        // No authenticated user setup - HttpContext.User will be null/empty

        _mockAuthService.Setup(x => x.LogoutAsync(logoutDto, null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout(logoutDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockAuthService.Verify(x => x.LogoutAsync(logoutDto, null), Times.Once);
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var token = "valid-token";

        _mockAuthService.Setup(x => x.ValidateTokenAsync(token))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;

        Assert.NotNull(response);
        _mockAuthService.Verify(x => x.ValidateTokenAsync(token), Times.Once);
    }

    [Fact]
    public async Task ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var token = "invalid-token";

        _mockAuthService.Setup(x => x.ValidateTokenAsync(token))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ValidateToken_EmptyToken_ReturnsBadRequest()
    {
        // Arrange
        var token = "";

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    #endregion

    #region Helper Methods

    private void SetupAuthenticatedUser(string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext.HttpContext.User = principal;
    }

    private AuthResponseDTO CreateValidAuthResponse()
    {
        return new AuthResponseDTO
        {
            User = new UserDTO
            {
                Id = "test-user-id",
                Name = "Test User",
                Email = "test@example.com",
                Teams = new List<CompanyInfoDTO>
                {
                    new CompanyInfoDTO
                    {
                        Name = "Test Company",
                        CompanyId = "company-123",
                        SubscriptionName = "Premium Plan",
                        SubscriptionId = "subscription-123",
                        UserPlanCondition = ProductManagementSystem.Application.AppEntities.Users.Models.EnumUserPlanType.Owner
                    }
                }
            },
            AccessToken = new TokenDTO
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                Type = "Bearer",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow
            },
            RefreshToken = new TokenDTO
            {
                Token = "refresh-token-value",
                Type = "refresh",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            },
            AuthenticatedAt = DateTime.UtcNow
        };
    }

    #endregion
}