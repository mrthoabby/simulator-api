using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.AppEntities.Quotations.Services;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Controllers;

public class QuotationControllerTests
{
    private readonly Mock<IQuotationService> _mockQuotationService;
    private readonly Mock<ILogger<QuotationController>> _mockLogger;
    private readonly QuotationController _controller;

    public QuotationControllerTests()
    {
        _mockQuotationService = new Mock<IQuotationService>();
        _mockLogger = new Mock<ILogger<QuotationController>>();

        _controller = new QuotationController(
            _mockQuotationService.Object,
            _mockLogger.Object
        );
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreatedResult()
    {
        var productId = "product-123";
        var createDto = CreateValidCreateQuotationDTO(productId);
        var quotationDto = CreateValidQuotationDTO(productId);

        _mockQuotationService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(quotationDto);

        var result = await _controller.Create(productId, createDto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        createdResult.RouteValues?["id"].Should().Be(quotationDto.Id);
        createdResult.Value.Should().BeEquivalentTo(quotationDto);

        _mockQuotationService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WithMismatchedProductId_ShouldReturnBadRequest()
    {
        var productId = "product-123";
        var createDto = CreateValidCreateQuotationDTO("different-product-id");

        var result = await _controller.Create(productId, createDto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnOkResult()
    {
        var productId = "product-123";
        var quotationId = "quotation-456";
        var quotationDto = CreateValidQuotationDTO(productId);

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync(quotationDto);

        var result = await _controller.GetById(productId, quotationId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(quotationDto);

        _mockQuotationService.Verify(s => s.GetByIdAsync(quotationId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldThrowNotFoundException()
    {
        var productId = "product-123";
        var quotationId = "non-existing-id";

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync((QuotationDTO?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetById(productId, quotationId));
    }

    [Fact]
    public async Task GetById_WithMismatchedProductId_ShouldThrowNotFoundException()
    {
        var productId = "product-123";
        var quotationId = "quotation-456";
        var quotationDto = CreateValidQuotationDTO("different-product-id");

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync(quotationDto);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetById(productId, quotationId));
    }

    #endregion

    #region GetByProductId Tests

    [Fact]
    public async Task GetByProductId_WithValidProductId_ShouldReturnOkResult()
    {
        var productId = "product-123";
        var paginationConfig = new PaginationConfigDTO { Page = 1, PageSize = 10 };
        var paginatedResult = new PaginatedResult<QuotationDTO>
        {
            Items = new List<QuotationDTO> { CreateValidQuotationDTO(productId) },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _mockQuotationService.Setup(s => s.GetByProductIdAsync(productId, paginationConfig, null))
            .ReturnsAsync(paginatedResult);

        var result = await _controller.GetByProductId(productId, paginationConfig, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(paginatedResult);

        _mockQuotationService.Verify(s => s.GetByProductIdAsync(productId, paginationConfig, null), Times.Once);
    }

    #endregion

    #region GetAllByProductId Tests

    [Fact]
    public async Task GetAllByProductId_WithValidProductId_ShouldReturnOkResult()
    {
        var productId = "product-123";
        var quotations = new List<QuotationDTO> { CreateValidQuotationDTO(productId) };

        _mockQuotationService.Setup(s => s.GetAllByProductIdAsync(productId))
            .ReturnsAsync(quotations);

        var result = await _controller.GetAllByProductId(productId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(quotations);

        _mockQuotationService.Verify(s => s.GetAllByProductIdAsync(productId), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ShouldReturnOkResult()
    {
        var productId = "product-123";
        var quotationId = "quotation-456";
        var updateDto = new UpdateQuotationDTO { UnitsPerBox = 50 };
        var existingQuotation = CreateValidQuotationDTO(productId);
        var updatedQuotation = CreateValidQuotationDTO(productId);

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync(existingQuotation);
        _mockQuotationService.Setup(s => s.UpdateAsync(quotationId, updateDto))
            .ReturnsAsync(updatedQuotation);

        var result = await _controller.Update(productId, quotationId, updateDto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(updatedQuotation);

        _mockQuotationService.Verify(s => s.UpdateAsync(quotationId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonExistingQuotation_ShouldThrowNotFoundException()
    {
        var productId = "product-123";
        var quotationId = "non-existing-id";
        var updateDto = new UpdateQuotationDTO { UnitsPerBox = 50 };

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync((QuotationDTO?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.Update(productId, quotationId, updateDto));
    }

    [Fact]
    public async Task Update_WithMismatchedProductId_ShouldThrowNotFoundException()
    {
        var productId = "product-123";
        var quotationId = "quotation-456";
        var updateDto = new UpdateQuotationDTO { UnitsPerBox = 50 };
        var existingQuotation = CreateValidQuotationDTO("different-product-id");

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync(existingQuotation);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.Update(productId, quotationId, updateDto));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContentResult()
    {
        var productId = "product-123";
        var quotationId = "quotation-456";
        var existingQuotation = CreateValidQuotationDTO(productId);

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync(existingQuotation);
        _mockQuotationService.Setup(s => s.DeleteAsync(quotationId))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(productId, quotationId);

        result.Should().BeOfType<NoContentResult>();

        _mockQuotationService.Verify(s => s.DeleteAsync(quotationId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistingQuotation_ShouldThrowNotFoundException()
    {
        var productId = "product-123";
        var quotationId = "non-existing-id";

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync((QuotationDTO?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.Delete(productId, quotationId));
    }

    [Fact]
    public async Task Delete_WithMismatchedProductId_ShouldThrowNotFoundException()
    {
        var productId = "product-123";
        var quotationId = "quotation-456";
        var existingQuotation = CreateValidQuotationDTO("different-product-id");

        _mockQuotationService.Setup(s => s.GetByIdAsync(quotationId))
            .ReturnsAsync(existingQuotation);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.Delete(productId, quotationId));
    }

    #endregion

    #region Helper Methods

    private static CreateQuotationDTO CreateValidCreateQuotationDTO(string productId)
    {
        return new CreateQuotationDTO
        {
            ProductId = productId,
            ProviderId = "provider-456",
            Dimensions = new DimensionsDTO { Width = 10, Height = 20, Depth = 30 },
            UnitsPerBox = 10,
            TotalUnits = 100,
            IsActive = true
        };
    }

    private static QuotationDTO CreateValidQuotationDTO(string productId)
    {
        return new QuotationDTO
        {
            Id = "quotation-456",
            ProductId = productId,
            ProviderId = "provider-456",
            ProviderName = "Test Provider",
            Dimensions = new DimensionsDTO { Width = 10, Height = 20, Depth = 30 },
            UnitsPerBox = 10,
            TotalUnits = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}

