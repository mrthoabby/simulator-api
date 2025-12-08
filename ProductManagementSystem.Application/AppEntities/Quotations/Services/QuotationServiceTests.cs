using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using ProductManagementSystem.Application.AppEntities.Quotations.Repository;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Quotations.Models;
using ProductManagementSystem.Application.AppEntities.Products.Repository;
using ProductManagementSystem.Application.AppEntities.Products.Models;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Services;

public class QuotationServiceTests
{
    private readonly Mock<IQuotationRepository> _mockQuotationRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<QuotationService>> _mockLogger;
    private readonly QuotationService _service;

    public QuotationServiceTests()
    {
        _mockQuotationRepository = new Mock<IQuotationRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<QuotationService>>();

        _service = new QuotationService(
            _mockQuotationRepository.Object,
            _mockProductRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsQuotationDTO()
    {
        var productId = "product-123";
        var providerId = "provider-456";
        var product = CreateTestProductWithProvider(productId, providerId);
        var createDto = CreateValidCreateQuotationDTO(productId, providerId);
        var quotation = CreateTestQuotation(productId, providerId);
        var quotationDto = CreateTestQuotationDTO(productId, providerId);

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockQuotationRepository.Setup(r => r.CreateAsync(It.IsAny<Quotation>()))
            .ReturnsAsync(quotation);
        _mockMapper.Setup(m => m.Map<QuotationDTO>(It.IsAny<Quotation>()))
            .Returns(quotationDto);

        var result = await _service.CreateAsync(createDto);

        result.Should().NotBeNull();
        result.ProductId.Should().Be(productId);
        result.ProviderId.Should().Be(providerId);
        _mockQuotationRepository.Verify(r => r.CreateAsync(It.IsAny<Quotation>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        var productId = "non-existing-product";
        var createDto = CreateValidCreateQuotationDTO(productId, "provider-123");

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingProvider_ThrowsNotFoundException()
    {
        var productId = "product-123";
        var providerId = "non-existing-provider";
        var product = CreateTestProductWithProvider(productId, "different-provider");
        var createDto = CreateValidCreateQuotationDTO(productId, providerId);

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(createDto));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsQuotationDTO()
    {
        var quotationId = "quotation-123";
        var quotation = CreateTestQuotation("product-123", "provider-456");
        var quotationDto = CreateTestQuotationDTO("product-123", "provider-456");

        _mockQuotationRepository.Setup(r => r.GetByIdAsync(quotationId))
            .ReturnsAsync(quotation);
        _mockMapper.Setup(m => m.Map<QuotationDTO>(quotation))
            .Returns(quotationDto);

        var result = await _service.GetByIdAsync(quotationId);

        result.Should().NotBeNull();
        _mockQuotationRepository.Verify(r => r.GetByIdAsync(quotationId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var quotationId = "non-existing-id";

        _mockQuotationRepository.Setup(r => r.GetByIdAsync(quotationId))
            .ReturnsAsync((Quotation?)null);

        var result = await _service.GetByIdAsync(quotationId);

        result.Should().BeNull();
    }

    #endregion

    #region GetByProductIdAsync Tests

    [Fact]
    public async Task GetByProductIdAsync_WithExistingProduct_ReturnsPaginatedResults()
    {
        var productId = "product-123";
        var product = CreateTestProductWithProvider(productId, "provider-456");
        var paginationConfig = new PaginationConfigDTO { Page = 1, PageSize = 10 };
        var quotations = new List<Quotation> { CreateTestQuotation(productId, "provider-456") };
        var quotationDtos = new List<QuotationDTO> { CreateTestQuotationDTO(productId, "provider-456") };

        var paginatedResult = new PaginatedResult<Quotation>
        {
            Items = quotations,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<PaginationConfigs>(paginationConfig))
            .Returns(PaginationConfigs.Create(1, 10));
        _mockQuotationRepository.Setup(r => r.GetByProductIdAsync(productId, It.IsAny<PaginationConfigs>(), null))
            .ReturnsAsync(paginatedResult);
        _mockMapper.Setup(m => m.Map<List<QuotationDTO>>(quotations))
            .Returns(quotationDtos);

        var result = await _service.GetByProductIdAsync(productId, paginationConfig);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByProductIdAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        var productId = "non-existing-product";
        var paginationConfig = new PaginationConfigDTO { Page = 1, PageSize = 10 };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetByProductIdAsync(productId, paginationConfig));
    }

    #endregion

    #region GetAllByProductIdAsync Tests

    [Fact]
    public async Task GetAllByProductIdAsync_WithExistingProduct_ReturnsQuotations()
    {
        var productId = "product-123";
        var product = CreateTestProductWithProvider(productId, "provider-456");
        var quotations = new List<Quotation> { CreateTestQuotation(productId, "provider-456") };
        var quotationDtos = new List<QuotationDTO> { CreateTestQuotationDTO(productId, "provider-456") };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockQuotationRepository.Setup(r => r.GetAllByProductIdAsync(productId))
            .ReturnsAsync(quotations);
        _mockMapper.Setup(m => m.Map<List<QuotationDTO>>(quotations))
            .Returns(quotationDtos);

        var result = await _service.GetAllByProductIdAsync(productId);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllByProductIdAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        var productId = "non-existing-product";

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetAllByProductIdAsync(productId));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithExistingQuotation_ReturnsUpdatedQuotationDTO()
    {
        var quotationId = "quotation-123";
        var updateDto = new UpdateQuotationDTO
        {
            UnitsPerBox = 50,
            TotalUnits = 500
        };
        var existingQuotation = CreateTestQuotation("product-123", "provider-456");
        var quotationDto = CreateTestQuotationDTO("product-123", "provider-456");

        _mockQuotationRepository.Setup(r => r.GetByIdAsync(quotationId))
            .ReturnsAsync(existingQuotation);
        _mockQuotationRepository.Setup(r => r.UpdateAsync(quotationId, It.IsAny<Quotation>()))
            .ReturnsAsync(existingQuotation);
        _mockMapper.Setup(m => m.Map<QuotationDTO>(existingQuotation))
            .Returns(quotationDto);

        var result = await _service.UpdateAsync(quotationId, updateDto);

        result.Should().NotBeNull();
        _mockQuotationRepository.Verify(r => r.UpdateAsync(quotationId, It.IsAny<Quotation>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingQuotation_ThrowsNotFoundException()
    {
        var quotationId = "non-existing-id";
        var updateDto = new UpdateQuotationDTO { UnitsPerBox = 50 };

        _mockQuotationRepository.Setup(r => r.GetByIdAsync(quotationId))
            .ReturnsAsync((Quotation?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.UpdateAsync(quotationId, updateDto));
    }

    [Fact]
    public async Task UpdateAsync_WithDimensions_UpdatesDimensions()
    {
        var quotationId = "quotation-123";
        var updateDto = new UpdateQuotationDTO
        {
            Dimensions = new DimensionsDTO { Width = 20, Height = 30, Depth = 40 }
        };
        var existingQuotation = CreateTestQuotation("product-123", "provider-456");
        var quotationDto = CreateTestQuotationDTO("product-123", "provider-456");

        _mockQuotationRepository.Setup(r => r.GetByIdAsync(quotationId))
            .ReturnsAsync(existingQuotation);
        _mockQuotationRepository.Setup(r => r.UpdateAsync(quotationId, It.IsAny<Quotation>()))
            .ReturnsAsync(existingQuotation);
        _mockMapper.Setup(m => m.Map<QuotationDTO>(existingQuotation))
            .Returns(quotationDto);

        var result = await _service.UpdateAsync(quotationId, updateDto);

        result.Should().NotBeNull();
        _mockQuotationRepository.Verify(r => r.UpdateAsync(quotationId, It.IsAny<Quotation>()), Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        var quotationId = "quotation-123";

        _mockQuotationRepository.Setup(r => r.DeleteAsync(quotationId))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(quotationId);

        _mockQuotationRepository.Verify(r => r.DeleteAsync(quotationId), Times.Once);
    }

    #endregion

    #region DeleteByProductIdAsync Tests

    [Fact]
    public async Task DeleteByProductIdAsync_CallsRepositoryDelete()
    {
        var productId = "product-123";

        _mockQuotationRepository.Setup(r => r.DeleteByProductIdAsync(productId))
            .Returns(Task.CompletedTask);

        await _service.DeleteByProductIdAsync(productId);

        _mockQuotationRepository.Verify(r => r.DeleteByProductIdAsync(productId), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Product CreateTestProductWithProvider(string productId, string providerId)
    {
        var product = Product.Create("Test Product").Build();
        
        // Use reflection to set the Id since it's private
        var idProperty = typeof(Product).GetProperty("Id");
        idProperty?.SetValue(product, productId);

        // Create provider with the specific ID
        var provider = Provider.Create("Test Provider", "https://provider.com", new List<Offer>
        {
            Offer.Create("https://provider.com/offer", 
                ProductManagementSystem.Application.AppEntities.Shared.Type.Money.Create(100, 
                    ProductManagementSystem.Application.AppEntities.Shared.Enum.EnumCurrency.USD), 1)
        });
        
        // Set the provider ID
        var providerIdProperty = typeof(Provider).GetProperty("Id");
        providerIdProperty?.SetValue(provider, providerId);

        product.GetType().GetProperty("Providers")?.SetValue(product, new List<Provider> { provider });
        
        return product;
    }

    private static Quotation CreateTestQuotation(string productId, string providerId)
    {
        var dimensions = Dimensions.Create(10, 20, 30);
        return Quotation.Create(productId, providerId, "Test Provider", dimensions, 10, 100, true);
    }

    private static CreateQuotationDTO CreateValidCreateQuotationDTO(string productId, string providerId)
    {
        return new CreateQuotationDTO
        {
            ProductId = productId,
            ProviderId = providerId,
            Dimensions = new DimensionsDTO { Width = 10, Height = 20, Depth = 30 },
            UnitsPerBox = 10,
            TotalUnits = 100,
            IsActive = true
        };
    }

    private static QuotationDTO CreateTestQuotationDTO(string productId, string providerId)
    {
        return new QuotationDTO
        {
            Id = "quotation-123",
            ProductId = productId,
            ProviderId = providerId,
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

