using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using ProductManagementSystem.Application.Domain.Users.Repository;
using ProductManagementSystem.Application.Domain.Users.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Users.Models;
using ProductManagementSystem.Application.Domain.UserPlans.Repository;
using ProductManagementSystem.Application.Domain.UserPlans.Domain;
using ProductManagementSystem.Application.Domain.UserPlans.Models;
using ProductManagementSystem.Application.Domain.Subscriptions.Repository;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;
using ProductManagementSystem.Application.Domain.Subscriptions.Enums;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Common;
using ProductManagementSystem.Application.Common.Domain.Enum;

namespace ProductManagementSystem.Application.Domain.Users.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserPlanRepository> _mockUserPlanRepository;
    private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly SecuritySettings _securitySettings;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserPlanRepository = new Mock<IUserPlanRepository>();
        _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _mockMapper = new Mock<IMapper>();

        // Create real SecuritySettings instance
        _securitySettings = new SecuritySettings
        {
            PasswordPepper = "test-pepper-for-unit-tests"
        };

        _service = new UserService(
            _mockUserRepository.Object,
            _mockUserPlanRepository.Object,
            _mockSubscriptionRepository.Object,
            _mockLogger.Object,
            _securitySettings,
            _mockMapper.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnUserDTO()
    {
        // Arrange
        var createDto = CreateValidCreateUserDTO();
        var subscription = CreateValidSubscription();
        var user = CreateValidUser();
        var userPlan = CreateValidUserPlan();
        var expectedUserDto = CreateValidUserDTO();

        _mockSubscriptionRepository.Setup(r => r.GetByIdAsync(createDto.SubscriptionId))
            .ReturnsAsync(subscription);

        _mockUserPlanRepository.Setup(r => r.GetAllWhereIsMemberAsync(createDto.Credentials.Email))
            .ReturnsAsync(new List<UserPlan>());

        _mockUserRepository.Setup(r => r.GetByEmailAsync(createDto.Credentials.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        _mockUserPlanRepository.Setup(r => r.CreateAsync(It.IsAny<UserPlan>()))
            .ReturnsAsync(userPlan);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Email.Should().Be(createDto.Credentials.Email);
        result.Teams.Should().NotBeNull();

        _mockSubscriptionRepository.Verify(r => r.GetByIdAsync(createDto.SubscriptionId), Times.Once);
        _mockUserRepository.Verify(r => r.GetByEmailAsync(createDto.Credentials.Email), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingEmail_ShouldThrowConflictException()
    {
        // Arrange
        var createDto = CreateValidCreateUserDTO();
        var subscription = CreateValidSubscription();
        var existingUser = CreateValidUser();

        _mockSubscriptionRepository.Setup(r => r.GetByIdAsync(createDto.SubscriptionId))
            .ReturnsAsync(subscription);

        _mockUserPlanRepository.Setup(r => r.GetAllWhereIsMemberAsync(createDto.Credentials.Email))
            .ReturnsAsync(new List<UserPlan>());

        _mockUserRepository.Setup(r => r.GetByEmailAsync(createDto.Credentials.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(createDto));

        exception.Message.Should().Be("User with this email already exists");
        _mockSubscriptionRepository.Verify(r => r.GetByIdAsync(createDto.SubscriptionId), Times.Once);
        _mockUserRepository.Verify(r => r.GetByEmailAsync(createDto.Credentials.Email), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region ActivateAsync Tests

    [Fact]
    public async Task ActivateAsync_WithValidData_ShouldReturnUserDTO()
    {
        // Arrange
        var activateDto = CreateValidActivateUserDTO();
        var user = CreateValidUser();
        var userPlans = new List<UserPlan> { CreateValidUserPlan() };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(activateDto.Credentials.Email))
            .ReturnsAsync((User?)null); // User should NOT exist for activation

        _mockUserPlanRepository.Setup(r => r.GetAllWhereIsMemberAsync(activateDto.Credentials.Email))
            .ReturnsAsync(userPlans);

        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ActivateAsync(activateDto);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(activateDto.Credentials.Email);
        result.Teams.Should().NotBeNull();

        _mockUserRepository.Verify(r => r.GetByEmailAsync(activateDto.Credentials.Email), Times.Once);
        _mockUserPlanRepository.Verify(r => r.GetAllWhereIsMemberAsync(activateDto.Credentials.Email), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_WithExistingUser_ShouldThrowConflictException()
    {
        // Arrange
        var activateDto = CreateValidActivateUserDTO();
        var existingUser = CreateValidUser();

        _mockUserRepository.Setup(r => r.GetByEmailAsync(activateDto.Credentials.Email))
            .ReturnsAsync(existingUser); // User already exists

        _mockUserPlanRepository.Setup(r => r.GetAllWhereIsMemberAsync(activateDto.Credentials.Email))
            .ReturnsAsync(new List<UserPlan>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _service.ActivateAsync(activateDto));

        exception.Message.Should().Be("User already exists");
        _mockUserRepository.Verify(r => r.GetByEmailAsync(activateDto.Credentials.Email), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnUserDTO()
    {
        // Arrange
        var userId = "123e4567-e89b-12d3-a456-426614174000";
        var user = CreateValidUser();
        var userPlans = new List<UserPlan> { CreateValidUserPlan() };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUserPlanRepository.Setup(r => r.GetAllWhereIsOwnerAsync(user.Credential.Email))
            .ReturnsAsync(userPlans);

        _mockUserPlanRepository.Setup(r => r.GetAllWhereIsMemberAsync(user.Credential.Email))
            .ReturnsAsync(new List<UserPlan>());

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Id.Should().Be(user.Id);
        result.Value.Name.Should().Be(user.Name);
        result.Value.Email.Should().Be(user.Credential.Email);
        result.Value.Teams.Should().NotBeNull();

        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var userId = "non-existing-id";

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUserDTO()
    {
        // Arrange
        var email = "test@example.com";
        var user = CreateValidUser();
        var userPlans = new List<UserPlan> { CreateValidUserPlan() };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _mockUserPlanRepository.Setup(r => r.GetAllWhereExistsAsync(email))
            .ReturnsAsync(userPlans);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Value.Email.Should().Be(email);
        result.Value.Teams.Should().NotBeNull();

        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _mockUserPlanRepository.Verify(r => r.GetAllWhereExistsAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var email = "nonexisting@example.com";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithValidFilter_ShouldReturnPaginatedResult()
    {
        // Arrange
        var filter = new UserFilterDTO
        {
            Page = 1,
            PageSize = 10,
            Name = "Test",
            Email = "test@example.com"
        };

        var users = new List<User> { CreateValidUser() };

        var paginatedResult = new PaginatedResult<User>
        {
            Items = users,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockUserRepository.Setup(r => r.GetAllAsync(
            It.Is<PaginationConfigs>(p => p.Page == 1 && p.PageSize == 10),
            filter))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        _mockUserRepository.Verify(r => r.GetAllAsync(It.IsAny<PaginationConfigs>(), filter), Times.Once);
    }

    #endregion

    #region GetAllNoPaginationAsync Tests

    [Fact]
    public async Task GetAllNoPaginationAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var filter = new UserFilterDTO();
        var users = new List<User> { CreateValidUser(), CreateValidUser() };
        var userDtos = new List<UserDTO> { CreateValidUserDTO(), CreateValidUserDTO() };

        _mockUserRepository.Setup(r => r.GetAllNoPaginationAsync(filter))
            .ReturnsAsync(users);

        _mockMapper.Setup(m => m.Map<List<UserDTO>>(users))
            .Returns(userDtos);

        // Act
        var result = await _service.GetAllNoPaginationAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockUserRepository.Verify(r => r.GetAllNoPaginationAsync(filter), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static CreateUserDTO CreateValidCreateUserDTO()
    {
        return new CreateUserDTO
        {
            Name = "Test User",
            Credentials = new CredentialsDTO
            {
                Email = "test@example.com",
                Password = "SecurePassword123!"
            },
            SubscriptionId = "123e4567-e89b-12d3-a456-426614174000",
            CompanyName = "Test Company"
        };
    }

    private static ActivateUserDTO CreateValidActivateUserDTO()
    {
        return new ActivateUserDTO
        {
            Name = "Test User",
            Credentials = new CredentialsDTO
            {
                Email = "test@example.com",
                Password = "SecurePassword123!"
            }
        };
    }

    private static User CreateValidUser()
    {
        var credential = Credential.FromPersistence("test@example.com", "hashed-password");
        return User.Create("Test User", credential);
    }

    private static Subscription CreateValidSubscription()
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

    private static UserPlan CreateValidUserPlan()
    {
        var subscription = CreateValidSubscription();
        var company = Company.Create("Test Company");
        return UserPlan.Create(subscription, company, "test@example.com");
    }

    private static UserDTO CreateValidUserDTO()
    {
        return new UserDTO
        {
            Id = "123e4567-e89b-12d3-a456-426614174000",
            Name = "Test User",
            Email = "test@example.com",
            Teams = new List<CompanyInfoDTO>
            {
                new CompanyInfoDTO
                {
                    Name = "Test Company",
                    CompanyId = "company-123",
                    SubscriptionName = "Premium Plan",
                    SubscriptionId = "123e4567-e89b-12d3-a456-426614174000",
                    UserPlanCondition = EnumUserPlanType.Owner
                }
            }
        };
    }

    #endregion
}