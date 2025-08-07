using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using AutoMapper;
using ProductManagementSystem.Application.Domain.Products.Services;
using ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Products.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProductController>>();

        _controller = new ProductController(
            _mockProductService.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = CreateValidCreateProductDTO();
        var productDto = CreateValidProductDTO();

        _mockProductService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(productDto);

        _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<object>()))
            .Returns(productDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues?["id"].Should().Be(productDto.Id);
        createdResult.Value.Should().BeEquivalentTo(productDto);

        _mockProductService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithValidationError_ShouldThrowValidationException()
    {
        // Arrange
        var createDto = CreateValidCreateProductDTO();
        var validationException = new ValidationException("Validation failed");

        _mockProductService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(validationException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _controller.Create(createDto));

        exception.Message.Should().Be("Validation failed");
        _mockProductService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnOkResult()
    {
        // Arrange
        var productId = "123e4567-e89b-12d3-a456-426614174000";
        var productDto = CreateValidProductDTO();

        _mockProductService.Setup(s => s.GetByIdAsync(productId))
            .ReturnsAsync(productDto);

        _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<object>()))
            .Returns(productDto);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(productDto);

        _mockProductService.Verify(s => s.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldThrowNotFoundException()
    {
        // Arrange
        var productId = "non-existing-id";

        _mockProductService.Setup(s => s.GetByIdAsync(productId))
            .ReturnsAsync((ProductDTO?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetById(productId));

        exception.Message.Should().Be("Product not found");
        _mockProductService.Verify(s => s.GetByIdAsync(productId), Times.Once);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidFilter_ShouldReturnOkResult()
    {
        // Arrange
        var paginationConfigs = new PaginationConfigDTO { Page = 1, PageSize = 10 };
        var filter = new FilterProductDTO { MinPrice = 100, MaxPrice = 1000, Currency = EnumCurrency.USD };
        var paginatedResult = new PaginatedResult<ProductDTO>
        {
            Items = new List<ProductDTO> { CreateValidProductDTO() },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _mockProductService.Setup(s => s.GetAllAsync(PaginationConfigs.Create(paginationConfigs.Page, paginationConfigs.PageSize), filter, null))
        .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAll(paginationConfigs, filter, null);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(paginatedResult);

        _mockProductService.Verify(s => s.GetAllAsync(PaginationConfigs.Create(paginationConfigs.Page, paginationConfigs.PageSize), filter, null), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var productId = "123e4567-e89b-12d3-a456-426614174000";
        var updateDto = CreateValidUpdateProductDTO();
        var productDto = CreateValidProductDTO();

        _mockProductService.Setup(s => s.UpdateAsync(productId, updateDto))
            .ReturnsAsync(productDto);

        _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<object>()))
            .Returns(productDto);

        // Act
        var result = await _controller.Update(productId, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(productDto);

        _mockProductService.Verify(s => s.UpdateAsync(productId, updateDto), Times.Once);
    }

    #endregion

    #region UpdateImage Tests

    [Fact]
    public async Task UpdateImage_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var productId = "123e4567-e89b-12d3-a456-426614174000";
        var updateImageDto = CreateValidUpdateProductImageDTO();
        var productDto = CreateValidProductDTO();

        _mockProductService.Setup(s => s.UpdateImageAsync(productId, updateImageDto))
            .ReturnsAsync(productDto);

        _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<object>()))
            .Returns(productDto);

        // Act
        var result = await _controller.UpdateImage(productId, updateImageDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(productDto);

        _mockProductService.Verify(s => s.UpdateImageAsync(productId, updateImageDto), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContentResult()
    {
        // Arrange
        var productId = "123e4567-e89b-12d3-a456-426614174000";

        _mockProductService.Setup(s => s.DeleteAsync(productId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mockProductService.Verify(s => s.DeleteAsync(productId), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static CreateProductDTO CreateValidCreateProductDTO()
    {
        return new CreateProductDTO
        {
            Name = "Test Product",
            Price = new MoneyDTO { Value = 99.99m, Currency = EnumCurrency.USD }
        };
    }

    private static UpdateProductDTO CreateValidUpdateProductDTO()
    {
        return new UpdateProductDTO
        {
            Name = "Updated Test Product",
            Price = new MoneyDTO { Value = 149.99m, Currency = EnumCurrency.USD }
        };
    }

    private static UpdateProductImageDTO CreateValidUpdateProductImageDTO()
    {
        return new UpdateProductImageDTO
        {
            ImageUrl = "https://example.com/image.jpg"
        };
    }

    private static ProductDTO CreateValidProductDTO()
    {
        return new ProductDTO
        {
            Id = "123e4567-e89b-12d3-a456-426614174000",
            Name = "Test Product",
            Price = new MoneyDTO { Value = 99.99m, Currency = EnumCurrency.USD },
            ImageUrl = "https://example.com/image.jpg",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}