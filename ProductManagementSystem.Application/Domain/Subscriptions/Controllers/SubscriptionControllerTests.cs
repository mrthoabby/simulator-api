using Xunit;
using Moq;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using ProductManagementSystem.Application.Domain.Subscriptions.Services;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Users.Services;
using ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Common.Domain.Enum;
using ProductManagementSystem.Application.Domain.Subscriptions.Enums;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Controllers;

public class SubscriptionControllerTests
{
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<SubscriptionController>> _mockLogger;
    private readonly SubscriptionController _controller;

    public SubscriptionControllerTests()
    {
        _mockSubscriptionService = new Mock<ISubscriptionService>();
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<SubscriptionController>>();

        _controller = new SubscriptionController(
            _mockSubscriptionService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockUserService.Object
        );
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = CreateValidCreateSubscriptionDTO();
        var subscriptionDto = CreateValidSubscriptionDTO();

        _mockSubscriptionService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(subscriptionDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues?["id"].Should().Be(subscriptionDto.Id);
        createdResult.Value.Should().BeEquivalentTo(subscriptionDto);

        _mockSubscriptionService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = CreateValidCreateSubscriptionDTO();
        var validationException = new ValidationException("Validation failed");

        _mockSubscriptionService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(validationException);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequestResult.Value.Should().BeAssignableTo<object>().Subject;
        errorResponse.ToString().Should().Contain("Validation failed");

        _mockSubscriptionService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithBusinessRuleViolation_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = CreateValidCreateSubscriptionDTO();
        var businessException = new InvalidOperationException("A subscription with this name already exists");

        _mockSubscriptionService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(businessException);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequestResult.Value.Should().BeAssignableTo<object>().Subject;
        errorResponse.ToString().Should().Contain("A subscription with this name already exists");

        _mockSubscriptionService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithUnexpectedError_ShouldReturnInternalServerError()
    {
        // Arrange
        var createDto = CreateValidCreateSubscriptionDTO();
        var exception = new Exception("Unexpected error");

        _mockSubscriptionService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);

        _mockSubscriptionService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnOkResult()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var subscriptionDto = CreateValidSubscriptionDTO();

        _mockSubscriptionService.Setup(s => s.GetByIdAsync(subscriptionId))
            .ReturnsAsync(subscriptionDto);

        // Act
        var result = await _controller.GetById(subscriptionId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(subscriptionDto);

        _mockSubscriptionService.Verify(s => s.GetByIdAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = "non-existing-id";

        _mockSubscriptionService.Setup(s => s.GetByIdAsync(subscriptionId))
            .ReturnsAsync((SubscriptionDTO?)null);

        // Act
        var result = await _controller.GetById(subscriptionId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Subscription with ID {subscriptionId} not found");

        _mockSubscriptionService.Verify(s => s.GetByIdAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithUnexpectedError_ShouldReturnInternalServerError()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var exception = new Exception("Unexpected error");

        _mockSubscriptionService.Setup(s => s.GetByIdAsync(subscriptionId))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.GetById(subscriptionId);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);

        _mockSubscriptionService.Verify(s => s.GetByIdAsync(subscriptionId), Times.Once);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidFilter_ShouldReturnOkResult()
    {
        // Arrange
        var filter = new SubscriptionFilterDTO
        {
            Page = 1,
            PageSize = 10,
            Name = "Premium",
            Period = "Monthly",
            IsActive = true
        };

        var paginatedResult = new PaginatedResult<SubscriptionDTO>
        {
            Items = new List<SubscriptionDTO> { CreateValidSubscriptionDTO() },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockSubscriptionService.Setup(s => s.GetAllAsync(filter))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(paginatedResult);

        _mockSubscriptionService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithNullFilter_ShouldUseDefaultFilter()
    {
        // Arrange
        var expectedFilter = new SubscriptionFilterDTO { Page = 1, PageSize = 10 };
        var paginatedResult = new PaginatedResult<SubscriptionDTO>
        {
            Items = new List<SubscriptionDTO>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockSubscriptionService.Setup(s => s.GetAllAsync(It.IsAny<SubscriptionFilterDTO>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAll(null);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(paginatedResult);

        _mockSubscriptionService.Verify(s => s.GetAllAsync(It.Is<SubscriptionFilterDTO>(f =>
            f.Page == 1 && f.PageSize == 10)), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithUnexpectedError_ShouldReturnInternalServerError()
    {
        // Arrange
        var filter = new SubscriptionFilterDTO { Page = 1, PageSize = 10 };
        var exception = new Exception("Unexpected error");

        _mockSubscriptionService.Setup(s => s.GetAllAsync(filter))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);

        _mockSubscriptionService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidIdAndNoActiveUsers_ShouldReturnOkResult()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var emptyUserList = new List<UserDTO>();

        _mockUserService.Setup(u => u.GetAllNoPaginationAsync())
            .ReturnsAsync(emptyUserList);

        _mockSubscriptionService.Setup(s => s.DeleteAsync(subscriptionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(subscriptionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        response.ToString().Should().Contain("Subscription deleted successfully");

        _mockUserService.Verify(u => u.GetAllNoPaginationAsync(), Times.Once);
        _mockSubscriptionService.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithActiveUsers_ShouldReturnBadRequest()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var usersWithSubscription = new List<UserDTO>
        {
            new UserDTO {
                Id = "user1",
                SubscriptionId = subscriptionId,
                Email = "test@example.com",
                Name = "Test User",
                CompanyId = "company1",
                UserPlanId = "userplan1" // ✅ Agregar UserPlanId requerido
            }
        };

        _mockUserService.Setup(u => u.GetAllNoPaginationAsync())
            .ReturnsAsync(usersWithSubscription);

        // Act
        var result = await _controller.Delete(subscriptionId);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Cannot delete subscription with active users");

        _mockUserService.Verify(u => u.GetAllNoPaginationAsync(), Times.Once);
        _mockSubscriptionService.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var subscriptionId = "non-existing-id";
        var emptyUserList = new List<UserDTO>();
        var argumentException = new ArgumentException("Subscription not found");

        _mockUserService.Setup(u => u.GetAllNoPaginationAsync())
            .ReturnsAsync(emptyUserList);

        _mockSubscriptionService.Setup(s => s.DeleteAsync(subscriptionId))
            .ThrowsAsync(argumentException);

        // Act
        var result = await _controller.Delete(subscriptionId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var errorResponse = notFoundResult.Value.Should().BeAssignableTo<object>().Subject;
        errorResponse.ToString().Should().Contain("Subscription not found");

        _mockUserService.Verify(u => u.GetAllNoPaginationAsync(), Times.Once);
        _mockSubscriptionService.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithUnexpectedError_ShouldReturnInternalServerError()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var emptyUserList = new List<UserDTO>();
        var exception = new Exception("Unexpected error");

        _mockUserService.Setup(u => u.GetAllNoPaginationAsync())
            .ReturnsAsync(emptyUserList);

        _mockSubscriptionService.Setup(s => s.DeleteAsync(subscriptionId))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.Delete(subscriptionId);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);

        _mockUserService.Verify(u => u.GetAllNoPaginationAsync(), Times.Once);
        _mockSubscriptionService.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static CreateSubscriptionDTO CreateValidCreateSubscriptionDTO()
    {
        return new CreateSubscriptionDTO
        {
            Name = "Premium Plan",
            Description = "Premium subscription with all features",
            Price = 29.99m,
            Currency = EnumCurrency.USD,
            Period = EnumSubscriptionPeriod.Monthly,
            MaxProducts = 100,
            MaxUsers = 10,
            MaxCompetitors = 50,
            MaxCustomDeductions = 20,
            MaxSimulations = 1000,
            IsPDFExportSupported = true,
            IsSimulationComparisonSupported = true,
            IsExcelExportSupported = true
        };
    }

    private static SubscriptionDTO CreateValidSubscriptionDTO()
    {
        return new SubscriptionDTO
        {
            Id = "123e4567-e89b-12d3-a456-426614174000",
            Name = "Premium Plan",
            Description = "Premium subscription with all features",
            Price = new PriceDTO
            {
                Value = 29.99m,
                Currency = "USD"
            },
            Period = "Monthly",
            Restrictions = new RestrictionsDTO
            {
                MaxProducts = 100,
                MaxUsers = 10,
                MaxCompetitors = 50,
                MaxCustomDeductions = 20,
                MaxSimulations = 1000,
                IsPDFExportSupported = true,
                IsSimulationComparisonSupported = true,
                IsExcelExportSupported = true
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}