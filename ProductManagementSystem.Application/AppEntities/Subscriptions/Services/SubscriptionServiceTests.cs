using Xunit;
using Moq;
using AutoMapper;
using FluentAssertions;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Repository;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Models;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Enums;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;
using ProductManagementSystem.Application.AppEntities.Users.Services;
using ProductManagementSystem.Application.AppEntities.Users.DTOs.Outputs;
using Microsoft.Extensions.Logging;
using ProductManagementSystem.Application.Common.AppEntities.Type;

namespace ProductManagementSystem.Application.AppEntities.Subscriptions.Services;

public class SubscriptionServiceTests
{
    private readonly Mock<ISubscriptionRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
    private readonly SubscriptionService _service;

    public SubscriptionServiceTests()
    {
        _mockRepository = new Mock<ISubscriptionRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<SubscriptionService>>();
        _service = new SubscriptionService(_mockRepository.Object, _mockMapper.Object, _mockUserService.Object, _mockLogger.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnSubscriptionDTO()
    {
        // Arrange
        var createDto = new CreateSubscriptionDTO
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

        var subscription = CreateSubscriptionModel();
        var expectedDto = CreateSubscriptionDTO();

        _mockRepository.Setup(r => r.GetByNameAsync(createDto.Name))
            .ReturnsAsync((Subscription?)null);

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Subscription>()))
            .ReturnsAsync(subscription);

        _mockMapper.Setup(m => m.Map<SubscriptionDTO>(subscription))
            .Returns(expectedDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);

        _mockRepository.Verify(r => r.GetByNameAsync(createDto.Name), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Subscription>()), Times.Once);
        _mockMapper.Verify(m => m.Map<SubscriptionDTO>(subscription), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateSubscriptionDTO
        {
            Name = "Existing Plan",
            Description = "Test plan",
            Price = 19.99m,
            Currency = EnumCurrency.USD,
            Period = EnumSubscriptionPeriod.Monthly
        };

        var existingSubscription = CreateSubscriptionModel();

        _mockRepository.Setup(r => r.GetByNameAsync(createDto.Name))
            .ReturnsAsync(existingSubscription);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(createDto));

        exception.Message.Should().Be("A subscription with this name already exists");
        _mockRepository.Verify(r => r.GetByNameAsync(createDto.Name), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Subscription>()), Times.Never);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnSubscriptionDTO()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var subscription = CreateSubscriptionModel();
        var expectedDto = CreateSubscriptionDTO();

        _mockRepository.Setup(r => r.GetByIdAsync(subscriptionId))
            .ReturnsAsync(subscription);

        _mockMapper.Setup(m => m.Map<SubscriptionDTO>(subscription))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(subscriptionId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);

        _mockRepository.Verify(r => r.GetByIdAsync(subscriptionId), Times.Once);
        _mockMapper.Verify(m => m.Map<SubscriptionDTO>(subscription), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var subscriptionId = "non-existing-id";

        _mockRepository.Setup(r => r.GetByIdAsync(subscriptionId))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.GetByIdAsync(subscriptionId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(subscriptionId), Times.Once);
        _mockMapper.Verify(m => m.Map<SubscriptionDTO>(It.IsAny<Subscription>()), Times.Never);
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnSubscriptionDTO()
    {
        // Arrange
        var subscriptionName = "Premium Plan";
        var subscription = CreateSubscriptionModel();
        var expectedDto = CreateSubscriptionDTO();

        _mockRepository.Setup(r => r.GetByNameAsync(subscriptionName))
            .ReturnsAsync(subscription);

        _mockMapper.Setup(m => m.Map<SubscriptionDTO>(subscription))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetByNameAsync(subscriptionName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);

        _mockRepository.Verify(r => r.GetByNameAsync(subscriptionName), Times.Once);
        _mockMapper.Verify(m => m.Map<SubscriptionDTO>(subscription), Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_WithNonExistingName_ShouldReturnNull()
    {
        // Arrange
        var subscriptionName = "Non-existing Plan";

        _mockRepository.Setup(r => r.GetByNameAsync(subscriptionName))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _service.GetByNameAsync(subscriptionName);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByNameAsync(subscriptionName), Times.Once);
        _mockMapper.Verify(m => m.Map<SubscriptionDTO>(It.IsAny<Subscription>()), Times.Never);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithValidFilter_ShouldReturnPaginatedResult()
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

        var subscriptions = new List<Subscription> { CreateSubscriptionModel() };
        var subscriptionDTOs = new List<SubscriptionDTO> { CreateSubscriptionDTO() };

        var paginatedResult = new PaginatedResult<Subscription>
        {
            Items = subscriptions,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockRepository.Setup(r => r.GetAllAsync(filter))
            .ReturnsAsync(paginatedResult);

        _mockMapper.Setup(m => m.Map<List<SubscriptionDTO>>(subscriptions))
            .Returns(subscriptionDTOs);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();

        _mockRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
        _mockMapper.Verify(m => m.Map<List<SubscriptionDTO>>(It.IsAny<List<Subscription>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var filter = new SubscriptionFilterDTO { Page = 1, PageSize = 10 };

        var paginatedResult = new PaginatedResult<Subscription>
        {
            Items = new List<Subscription>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockRepository.Setup(r => r.GetAllAsync(filter))
            .ReturnsAsync(paginatedResult);

        _mockMapper.Setup(m => m.Map<List<SubscriptionDTO>>(It.IsAny<List<Subscription>>()))
            .Returns(new List<SubscriptionDTO>());

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

        _mockRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
        _mockMapper.Verify(m => m.Map<List<SubscriptionDTO>>(paginatedResult.Items), Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var subscriptionId = "123e4567-e89b-12d3-a456-426614174000";
        var subscription = CreateSubscriptionModel();

        _mockRepository.Setup(r => r.GetByIdAsync(subscriptionId))
            .ReturnsAsync(subscription);

        _mockUserService.Setup(u => u.GetAllNoPaginationAsync(It.IsAny<ProductManagementSystem.Application.AppEntities.Users.DTOs.Inputs.UserFilterDTO>()))
            .ReturnsAsync(new List<UserDTO>());

        _mockRepository.Setup(r => r.DeleteAsync(subscriptionId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(subscriptionId);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(subscriptionId), Times.Once);
        _mockUserService.Verify(u => u.GetAllNoPaginationAsync(It.IsAny<ProductManagementSystem.Application.AppEntities.Users.DTOs.Inputs.UserFilterDTO>()), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(subscriptionId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldThrowArgumentException()
    {
        // Arrange
        var subscriptionId = "non-existing-id";

        _mockRepository.Setup(r => r.GetByIdAsync(subscriptionId))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.DeleteAsync(subscriptionId));

        exception.Message.Should().Be("Subscription not found");
        _mockRepository.Verify(r => r.GetByIdAsync(subscriptionId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static Subscription CreateSubscriptionModel()
    {
        var price = new Price(29.99m, EnumCurrency.USD);
        var restrictions = Restrictions.Create(100, 10, 50, 20, 1000, true, true, true);

        return Subscription.Create(
            "Premium Plan",
            "Premium subscription with all features",
            price,
            EnumSubscriptionPeriod.Monthly,
            restrictions
        );
    }

    private static SubscriptionDTO CreateSubscriptionDTO()
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
