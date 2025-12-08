using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using ProductManagementSystem.Application.AppEntities.Products.Repository;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Products.Models;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using ProductManagementSystem.Application.AppEntities.Concepts.Domain;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.Products.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IConceptDomainRules> _mockConceptDomainRules;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockConceptDomainRules = new Mock<IConceptDomainRules>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProductService>>();

        _service = new ProductService(
            _mockRepository.Object,
            _mockConceptDomainRules.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsProductDTO()
    {
        var createDto = new CreateProductDTO
        {
            Name = "Test Product"
        };

        var product = CreateTestProduct();
        var productDto = CreateTestProductDTO();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<Product>()))
            .Returns(productDto);

        var result = await _service.CreateAsync(createDto);

        result.Should().NotBeNull();
        result.Name.Should().Be(productDto.Name);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithImageUrl_SetsImageUrl()
    {
        var createDto = new CreateProductDTO
        {
            Name = "Test Product",
            ImageUrl = "https://example.com/image.jpg"
        };

        var product = CreateTestProduct();
        var productDto = CreateTestProductDTO();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<Product>()))
            .Returns(productDto);

        var result = await _service.CreateAsync(createDto);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsProductDTO()
    {
        var productId = "test-id";
        var product = CreateTestProduct();
        var productDto = CreateTestProductDTO();

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDTO>(product))
            .Returns(productDto);

        var result = await _service.GetByIdAsync(productId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(productDto.Id);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var productId = "non-existing-id";

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(productId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepositoryThrows_RethrowsException()
    {
        var productId = "test-id";

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(productId));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var paginationConfig = new PaginationConfigDTO { Page = 1, PageSize = 10 };
        var products = new List<Product> { CreateTestProduct() };
        var productDtos = new List<ProductDTO> { CreateTestProductDTO() };

        var paginatedResult = new PaginatedResult<Product>
        {
            Items = products,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _mockMapper.Setup(m => m.Map<PaginationConfigs>(paginationConfig))
            .Returns(PaginationConfigs.Create(1, 10));
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<PaginationConfigs>(), null, null))
            .ReturnsAsync(paginatedResult);
        _mockMapper.Setup(m => m.Map<List<ProductDTO>>(products))
            .Returns(productDtos);

        var result = await _service.GetAllAsync(paginationConfig);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryThrows_RethrowsException()
    {
        var paginationConfig = new PaginationConfigDTO { Page = 1, PageSize = 10 };

        _mockMapper.Setup(m => m.Map<PaginationConfigs>(paginationConfig))
            .Returns(PaginationConfigs.Create(1, 10));
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<PaginationConfigs>(), null, null))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync(paginationConfig));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithExistingProduct_ReturnsUpdatedProductDTO()
    {
        var productId = "test-id";
        var updateDto = new UpdateProductDTO
        {
            Name = "Updated Product"
        };
        var existingProduct = CreateTestProduct();
        var updatedProduct = CreateTestProduct();
        var productDto = CreateTestProductDTO();

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.UpdateAsync(productId, It.IsAny<Product>()))
            .ReturnsAsync(updatedProduct);
        _mockMapper.Setup(m => m.Map<ProductDTO>(updatedProduct))
            .Returns(productDto);

        var result = await _service.UpdateAsync(productId, updateDto);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(productId, It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        var productId = "non-existing-id";
        var updateDto = new UpdateProductDTO { Name = "Updated Product" };

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(productId, updateDto));
    }

    #endregion

    #region UpdateImageAsync Tests

    [Fact]
    public async Task UpdateImageAsync_WithExistingProduct_ReturnsUpdatedProductDTO()
    {
        var productId = "test-id";
        var updateDto = new UpdateProductImageDTO { ImageUrl = "https://example.com/new-image.jpg" };
        var existingProduct = CreateTestProduct();
        var updatedProduct = CreateTestProduct();
        var productDto = CreateTestProductDTO();

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.UpdateAsync(productId, It.IsAny<Product>()))
            .ReturnsAsync(updatedProduct);
        _mockMapper.Setup(m => m.Map<ProductDTO>(updatedProduct))
            .Returns(productDto);

        var result = await _service.UpdateImageAsync(productId, updateDto);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(productId, It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateImageAsync_WithNonExistingProduct_ThrowsNotFoundException()
    {
        var productId = "non-existing-id";
        var updateDto = new UpdateProductImageDTO { ImageUrl = "https://example.com/image.jpg" };

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateImageAsync(productId, updateDto));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        var productId = "test-id";

        _mockRepository.Setup(r => r.DeleteAsync(productId))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(productId);

        _mockRepository.Verify(r => r.DeleteAsync(productId), Times.Once);
    }

    #endregion

    #region Provider Operations Tests


    [Fact]
    public async Task RemoveProviderAsync_CallsRepositoryRemove()
    {
        var productId = "test-id";
        var providerName = "Test Provider";

        _mockRepository.Setup(r => r.RemoveProviderAsync(productId, providerName))
            .Returns(Task.CompletedTask);

        await _service.RemoveProviderAsync(productId, providerName);

        _mockRepository.Verify(r => r.RemoveProviderAsync(productId, providerName), Times.Once);
    }

    [Fact]
    public async Task GetProvidersAsync_CallsRepository()
    {
        var productId = "test-id";
        var offer = Offer.Create("https://provider.com/offer", Money.Create(50m, EnumCurrency.USD), 1);
        var providers = new List<Provider> { Provider.Create("Test", "https://test.com", new List<Offer> { offer }) };

        _mockRepository.Setup(r => r.GetProvidersAsync(productId)).ReturnsAsync(providers);
        _mockMapper.Setup(m => m.Map<List<ProviderDTO>>(providers)).Returns(new List<ProviderDTO>());

        await _service.GetProvidersAsync(productId);

        _mockRepository.Verify(r => r.GetProvidersAsync(productId), Times.Once);
    }

    #endregion

    #region Competitor Operations Tests

    [Fact]
    public async Task AddCompetitorAsync_CallsRepository()
    {
        var productId = "test-id";
        var addDto = new AddCompetitorDTO
        {
            Name = "Competitor",
            Url = "https://competitor.com",
            Price = new MoneyDTO { Value = 89.99m, Currency = EnumCurrency.USD }
        };
        var competitor = Competitor.Create("Competitor", Money.Create(89.99m, EnumCurrency.USD), "https://competitor.com", null);

        _mockRepository.Setup(r => r.AddCompetitorAsync(productId, It.IsAny<Competitor>())).ReturnsAsync(competitor);
        _mockMapper.Setup(m => m.Map<CompetitorDTO>(competitor)).Returns(It.IsAny<CompetitorDTO>());

        await _service.AddCompetitorAsync(productId, addDto);

        _mockRepository.Verify(r => r.AddCompetitorAsync(productId, It.IsAny<Competitor>()), Times.Once);
    }

    [Fact]
    public async Task RemoveCompetitorAsync_CallsRepositoryRemove()
    {
        var productId = "test-id";
        var competitorUrl = "https://competitor.com";

        _mockRepository.Setup(r => r.RemoveCompetitorAsync(productId, competitorUrl))
            .Returns(Task.CompletedTask);

        await _service.RemoveCompetitorAsync(productId, competitorUrl);

        _mockRepository.Verify(r => r.RemoveCompetitorAsync(productId, competitorUrl), Times.Once);
    }

    [Fact]
    public async Task GetCompetitorsAsync_CallsRepository()
    {
        var productId = "test-id";
        var competitors = new List<Competitor>();

        _mockRepository.Setup(r => r.GetCompetitorsAsync(productId)).ReturnsAsync(competitors);
        _mockMapper.Setup(m => m.Map<List<CompetitorDTO>>(competitors)).Returns(new List<CompetitorDTO>());

        await _service.GetCompetitorsAsync(productId);

        _mockRepository.Verify(r => r.GetCompetitorsAsync(productId), Times.Once);
    }

    #endregion

    #region Concept Operations Tests

    [Fact]
    public async Task RemoveConceptAsync_CallsRepositoryRemove()
    {
        var productId = "test-id";
        var conceptCode = "TEST-CODE";

        _mockRepository.Setup(r => r.RemoveConceptAsync(productId, conceptCode))
            .Returns(Task.CompletedTask);

        await _service.RemoveConceptAsync(productId, conceptCode);

        _mockRepository.Verify(r => r.RemoveConceptAsync(productId, conceptCode), Times.Once);
    }

    [Fact]
    public async Task GetConceptsAsync_CallsRepository()
    {
        var productId = "test-id";
        var concepts = new List<Concept>();

        _mockRepository.Setup(r => r.GetConceptsAsync(productId)).ReturnsAsync(concepts);
        _mockMapper.Setup(m => m.Map<List<ConceptDTO>>(concepts)).Returns(new List<ConceptDTO>());

        await _service.GetConceptsAsync(productId);

        _mockRepository.Verify(r => r.GetConceptsAsync(productId), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Product CreateTestProduct()
    {
        return Product.Create("Test Product").Build();
    }

    private static ProductDTO CreateTestProductDTO()
    {
        return new ProductDTO
        {
            Id = "test-id",
            Name = "Test Product",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
