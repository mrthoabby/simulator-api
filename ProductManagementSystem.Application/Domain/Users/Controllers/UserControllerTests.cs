using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductManagementSystem.Application.Domain.Users.Services;
using ProductManagementSystem.Application.Domain.Users.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Common.Domain.Errors;
using ProductManagementSystem.Application.Domain.Users.Models;

namespace ProductManagementSystem.Application.Domain.Users.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();

        _controller = new UserController(_mockUserService.Object, _mockLogger.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = CreateValidCreateUserDTO();
        var userDto = CreateValidUserDTO();

        _mockUserService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues?["id"].Should().Be(userDto.Id);
        createdResult.Value.Should().BeEquivalentTo(userDto);

        _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithConflictException_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = CreateValidCreateUserDTO();
        var conflictException = new ConflictException("User with this email already exists");

        _mockUserService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(conflictException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _controller.Create(createDto));

        exception.Message.Should().Be("User with this email already exists");
        _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithNotFoundException_ShouldThrowNotFoundException()
    {
        // Arrange
        var createDto = CreateValidCreateUserDTO();
        var notFoundException = new NotFoundException("Subscription not found");

        _mockUserService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(notFoundException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.Create(createDto));

        exception.Message.Should().Be("Subscription not found");
        _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldCallServiceAndReturnCreated()
    {
        // Arrange
        var createDto = CreateValidCreateUserDTO();
        var userDto = CreateValidUserDTO();

        _mockUserService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues?["id"].Should().Be(userDto.Id);
        createdResult.Value.Should().BeEquivalentTo(userDto);

        _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public async Task Activate_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var activateDto = CreateValidActivateUserDTO();
        var userDto = CreateValidUserDTO();

        _mockUserService.Setup(s => s.ActivateAsync(activateDto))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.Activate(activateDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues?["id"].Should().Be(userDto.Id);
        createdResult.Value.Should().BeEquivalentTo(userDto);

        _mockUserService.Verify(s => s.ActivateAsync(activateDto), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnOkResult()
    {
        // Arrange
        var userId = "123e4567-e89b-12d3-a456-426614174000";
        var userDto = CreateValidUserDTO();

        _mockUserService.Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(userDto);

        _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "non-existing-id";

        _mockUserService.Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync((UserDTO?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetById(userId));

        exception.Message.Should().Be("User not found");
        _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithEmptyId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "";

        _mockUserService.Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync((UserDTO?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetById(userId));

        exception.Message.Should().Be("User not found");
        _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithException_ShouldPropagateException()
    {
        // Arrange
        var userId = "123e4567-e89b-12d3-a456-426614174000";
        var exception = new Exception("Database connection failed");

        _mockUserService.Setup(s => s.GetByIdAsync(userId))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(
            () => _controller.GetById(userId));

        thrownException.Message.Should().Be("Database connection failed");
        _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidFilter_ShouldReturnOkResult()
    {
        // Arrange
        var filter = new UserFilterDTO
        {
            Page = 1,
            PageSize = 10,
            Name = "Test",
            Email = "test@example.com"
        };

        var paginatedResult = new PaginatedResult<UserDTO>
        {
            Items = new List<UserDTO> { CreateValidUserDTO() },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockUserService.Setup(s => s.GetAllAsync(filter))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(paginatedResult);

        _mockUserService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithEmptyResult_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var filter = new UserFilterDTO { Page = 1, PageSize = 10 };

        var paginatedResult = new PaginatedResult<UserDTO>
        {
            Items = new List<UserDTO>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockUserService.Setup(s => s.GetAllAsync(filter))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var resultValue = okResult.Value.Should().BeAssignableTo<PaginatedResult<UserDTO>>().Subject;
        resultValue.Items.Should().BeEmpty();
        resultValue.TotalCount.Should().Be(0);

        _mockUserService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithPagination_ShouldReturnPaginatedResult()
    {
        // Arrange
        var filter = new UserFilterDTO { Page = 2, PageSize = 5 };
        var paginatedResult = new PaginatedResult<UserDTO>
        {
            Items = new List<UserDTO> { CreateValidUserDTO() },
            TotalCount = 15,
            Page = 2,
            PageSize = 5
        };

        _mockUserService.Setup(s => s.GetAllAsync(filter))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(paginatedResult);
        _mockUserService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithException_ShouldPropagateException()
    {
        // Arrange
        var filter = new UserFilterDTO { Page = 1, PageSize = 10 };
        var exception = new Exception("Database connection failed");

        _mockUserService.Setup(s => s.GetAllAsync(filter))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(
            () => _controller.GetAll(filter));

        thrownException.Message.Should().Be("Database connection failed");
        _mockUserService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    #endregion

    #region Domain Logic Tests

    [Fact]
    public async Task Create_ShouldCallServiceWithCorrectData()
    {
        // Arrange
        var createDto = CreateValidCreateUserDTO();
        var userDto = CreateValidUserDTO();

        _mockUserService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnUserData()
    {
        // Arrange
        var userId = "123e4567-e89b-12d3-a456-426614174000";
        var userDto = CreateValidUserDTO();

        _mockUserService.Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(userDto);
        _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
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