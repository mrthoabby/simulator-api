using Xunit;
using Moq;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Services;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Users.Services;
using ProductManagementSystem.Application.AppEntities.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Enums;
using ProductManagementSystem.Application.AppEntities.Users.Models;
using ProductManagementSystem.Application.AppEntities.Users.DTOs.Inputs;

namespace ProductManagementSystem.Application.AppEntities.Subscriptions.Controllers;

public class SubscriptionControllerTests
{
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;
    private readonly Mock<ILogger<SubscriptionController>> _mockLogger;
    private readonly SubscriptionController _controller;

    public SubscriptionControllerTests()
    {
        _mockSubscriptionService = new Mock<ISubscriptionService>();
        _mockLogger = new Mock<ILogger<SubscriptionController>>();

        _controller = new SubscriptionController(
            _mockSubscriptionService.Object,
            _mockLogger.Object
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
    public async Task Create_WithValidationError_ShouldThrowValidationException()
    {
        // Arrange
        var createDto = CreateValidCreateSubscriptionDTO();
        var validationException = new ValidationException("Validation failed");

        _mockSubscriptionService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(validationException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _controller.Create(createDto));

        exception.Message.Should().Be("Validation failed");
        _mockSubscriptionService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithBusinessRuleViolation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createDto = CreateValidCreateSubscriptionDTO();
        var businessException = new InvalidOperationException("A subscription with this name already exists");

        _mockSubscriptionService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(businessException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Create(createDto));

        exception.Message.Should().Be("A subscription with this name already exists");
        _mockSubscriptionService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithUnexpectedError_ShouldThrowException()
    {
        // Arrange
        var createDto = CreateValidCreateSubscriptionDTO();
        var exception = new Exception("Unexpected error");

        _mockSubscriptionService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(
            () => _controller.Create(createDto));

        thrownException.Message.Should().Be("Unexpected error");
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
    public async Task GetById_WithUnexpectedError_ShouldThrowException()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var exception = new Exception("Unexpected error");

        _mockSubscriptionService.Setup(s => s.GetByIdAsync(subscriptionId))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(
            () => _controller.GetById(subscriptionId));

        thrownException.Message.Should().Be("Unexpected error");
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
        var result = await _controller.GetAll(expectedFilter);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(paginatedResult);

        _mockSubscriptionService.Verify(s => s.GetAllAsync(It.Is<SubscriptionFilterDTO>(f =>
            f.Page == 1 && f.PageSize == 10)), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithUnexpectedError_ShouldThrowException()
    {
        // Arrange
        var filter = new SubscriptionFilterDTO { Page = 1, PageSize = 10 };
        var exception = new Exception("Unexpected error");

        _mockSubscriptionService.Setup(s => s.GetAllAsync(filter))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(
            () => _controller.GetAll(filter));

        thrownException.Message.Should().Be("Unexpected error");
        _mockSubscriptionService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";

        _mockSubscriptionService.Setup(s => s.DeleteAsync(subscriptionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(subscriptionId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        response.ToString().Should().Contain("Subscription deleted successfully");

        _mockSubscriptionService.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithBusinessRuleViolation_ShouldThrowException()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var businessException = new InvalidOperationException("Cannot delete subscription with active users");

        _mockSubscriptionService.Setup(s => s.DeleteAsync(subscriptionId))
            .ThrowsAsync(businessException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Delete(subscriptionId));

        exception.Message.Should().Be("Cannot delete subscription with active users");
        _mockSubscriptionService.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldThrowArgumentException()
    {
        // Arrange
        var subscriptionId = "non-existing-id";
        var argumentException = new ArgumentException("Subscription not found");

        _mockSubscriptionService.Setup(s => s.DeleteAsync(subscriptionId))
            .ThrowsAsync(argumentException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _controller.Delete(subscriptionId));

        exception.Message.Should().Be("Subscription not found");
        _mockSubscriptionService.Verify(s => s.DeleteAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithUnexpectedError_ShouldThrowException()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var exception = new Exception("Unexpected error");

        _mockSubscriptionService.Setup(s => s.DeleteAsync(subscriptionId))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(
            () => _controller.Delete(subscriptionId));

        thrownException.Message.Should().Be("Unexpected error");
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