using ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Products.Repository;
using ProductManagementSystem.Application.AppEntities.Products.Models;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.AppEntities.Concepts.Domain;
using AutoMapper;
using ProductManagementSystem.Application.Common.Errors;


namespace ProductManagementSystem.Application.AppEntities.Products.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IConceptDomainRules _conceptDomainRules;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, IConceptDomainRules conceptDomainRules, IMapper mapper, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _conceptDomainRules = conceptDomainRules;
        _mapper = mapper;
        _logger = logger;
    }



    public async Task<ProductDTO> CreateAsync(CreateProductDTO dto)
    {
        _logger.LogInformation("Creating product: {Name}", dto.Name);
        var productBuilder = Product.Create(dto.Name);
        if (!string.IsNullOrEmpty(dto.ImageUrl))
            productBuilder.WithImageUrl(dto.ImageUrl);

        if (dto.Concepts != null && dto.Concepts.Any())
        {
            var concepts = _mapper.Map<List<Concept>>(dto.Concepts);
            await _conceptDomainRules.Validate(concepts);
            productBuilder.WithConcepts(concepts);
        }

        if (dto.Providers != null && dto.Providers.Any())
            productBuilder
            .WithProviders(_mapper
            .Map<List<Provider>>(dto.Providers));

        if (dto.Competitors != null && dto.Competitors.Any())
            productBuilder
            .WithCompetitors(_mapper
            .Map<List<Competitor>>(dto.Competitors));

        var product = productBuilder.Build();

        var createdProduct = await _productRepository.CreateAsync(product);
        var productDto = _mapper.Map<ProductDTO>(createdProduct);
        return productDto;

    }

    public async Task<ProductDTO?> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", id);
                return null;
            }

            var productDto = _mapper.Map<ProductDTO>(product);
            return productDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<PaginatedResult<ProductDTO>> GetAllAsync(PaginationConfigDTO paginationConfigs, FilterProductDTO? filter = null, string? search = null)
    {
        try
        {
            _logger.LogInformation("Getting all products with pagination: Page {Page}, PageSize {PageSize}",
                paginationConfigs.Page, paginationConfigs.PageSize);

            var paginationConfig = _mapper.Map<PaginationConfigs>(paginationConfigs);
            var paginatedProducts = await _productRepository.GetAllAsync(paginationConfig, filter, search);

            var productsDTO = _mapper.Map<List<ProductDTO>>(paginatedProducts.Items);

            var result = new PaginatedResult<ProductDTO>
            {
                Items = productsDTO,
                TotalCount = paginatedProducts.TotalCount,
                Page = paginatedProducts.Page,
                PageSize = paginatedProducts.PageSize,
                TotalPages = paginatedProducts.TotalPages,
                HasNextPage = paginatedProducts.HasNextPage,
                HasPreviousPage = paginatedProducts.HasPreviousPage
            };

            _logger.LogInformation("Retrieved {Count} products out of {TotalCount} total",
                productsDTO.Count, paginatedProducts.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw;
        }
    }

    public async Task<ProductDTO> UpdateAsync(string id, UpdateProductDTO dto)
    {

        _logger.LogInformation("Updating product with ID: {ProductId}", id);

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        var productBuilder = Product.Create(existingProduct.Name);
        if (!string.IsNullOrEmpty(dto.Name))
            productBuilder.WithName(dto.Name);

        if (!string.IsNullOrEmpty(dto.ImageUrl))
            productBuilder.WithImageUrl(dto.ImageUrl);

        // Validar y actualizar
        var validatedProduct = productBuilder.Build();
        var updatedProduct = await _productRepository.UpdateAsync(id, validatedProduct);

        var productDto = _mapper.Map<ProductDTO>(updatedProduct);

        _logger.LogInformation("Product updated successfully: {ProductId}", id);
        return productDto;

    }

    public async Task<ProductDTO> UpdateImageAsync(string id, UpdateProductImageDTO dto)
    {

        _logger.LogInformation("Updating image for product with ID: {ProductId}", id);

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        var productBuilder = Product.Create(existingProduct.Name);
        if (!string.IsNullOrEmpty(dto.ImageUrl))
            productBuilder.WithImageUrl(dto.ImageUrl);

        var validatedProduct = productBuilder.Build();
        var updatedProduct = await _productRepository.UpdateAsync(id, validatedProduct);

        var productDto = _mapper.Map<ProductDTO>(updatedProduct);

        _logger.LogInformation("Product image updated successfully: {ProductId}", id);
        return productDto;

    }

    public async Task DeleteAsync(string id)
    {

        _logger.LogInformation("Deleting product with ID: {ProductId}", id);

        await _productRepository.DeleteAsync(id);

        _logger.LogInformation("Product deleted successfully: {ProductId}", id);

    }

    // Concept operations
    public async Task<ConceptDTO> AddConceptAsync(string productId, AddConceptDTO dto)
    {

        _logger.LogInformation("Adding concept {ConceptCode} to product {ProductId}",
            dto.ConceptCode, productId);

        var concept = _mapper.Map<Concept>(dto);
        var addedConcept = await _productRepository.AddConceptAsync(productId, concept);

        var conceptDto = _mapper.Map<ConceptDTO>(addedConcept);

        _logger.LogInformation("Concept {ConceptCode} added successfully to product {ProductId}",
            dto.ConceptCode, productId);

        return conceptDto;

    }

    public async Task RemoveConceptAsync(string productId, string conceptCode)
    {

        _logger.LogInformation("Removing concept {ConceptCode} from product {ProductId}",
            conceptCode, productId);

        await _productRepository.RemoveConceptAsync(productId, conceptCode);

        _logger.LogInformation("Concept {ConceptCode} removed successfully from product {ProductId}",
        conceptCode, productId);

    }

    public async Task<List<ConceptDTO>> GetConceptsAsync(string productId)
    {

        _logger.LogInformation("Getting concepts for product {ProductId}", productId);

        var concepts = await _productRepository.GetConceptsAsync(productId);
        var conceptDtos = _mapper.Map<List<ConceptDTO>>(concepts);

        _logger.LogInformation("Retrieved {Count} concepts for product {ProductId}",
    conceptDtos.Count, productId);

        return conceptDtos;

    }

    // Provider operations
    public async Task<ProviderDTO> AddProviderAsync(string productId, AddProviderDTO dto)
    {

        _logger.LogInformation("Adding provider {ProviderName} to product {ProductId}",
            dto.Name, productId);

        var provider = Provider.Create(dto.Name, dto.Url, new List<Offer>());
        var addedProvider = await _productRepository.AddProviderAsync(productId, provider);

        var providerDto = _mapper.Map<ProviderDTO>(addedProvider);

        _logger.LogInformation("Provider {ProviderName} added successfully to product {ProductId}",
            dto.Name, productId);

        return providerDto;

    }

    public async Task RemoveProviderAsync(string productId, string providerName)
    {

        _logger.LogInformation("Removing provider {ProviderName} from product {ProductId}",
            providerName, productId);

        await _productRepository.RemoveProviderAsync(productId, providerName);

        _logger.LogInformation("Provider {ProviderName} removed successfully from product {ProductId}",
        providerName, productId);

    }

    public async Task<List<ProviderDTO>> GetProvidersAsync(string productId)
    {

        _logger.LogInformation("Getting providers for product {ProductId}", productId);

        var providers = await _productRepository.GetProvidersAsync(productId);
        var providerDtos = _mapper.Map<List<ProviderDTO>>(providers);

        _logger.LogInformation("Retrieved {Count} providers for product {ProductId}",
            providerDtos.Count, productId);

        return providerDtos;

    }

    // Competitor operations
    public async Task<CompetitorDTO> AddCompetitorAsync(string productId, AddCompetitorDTO dto)
    {

        _logger.LogInformation("Adding competitor with URL {CompetitorUrl} to product {ProductId}",
            dto.Url, productId);

        var price = Money.Create(dto.Price.Value, dto.Price.Currency);
        var competitor = Competitor.Create(dto.Name, price, dto.Url, dto.ImageUrl);
        var addedCompetitor = await _productRepository.AddCompetitorAsync(productId, competitor);

        var competitorDto = _mapper.Map<CompetitorDTO>(addedCompetitor);

        _logger.LogInformation("Competitor with URL {CompetitorUrl} added successfully to product {ProductId}",
            dto.Url, productId);

        return competitorDto;

    }

    public async Task RemoveCompetitorAsync(string productId, string competitorUrl)
    {

        _logger.LogInformation("Removing competitor with URL {CompetitorUrl} from product {ProductId}",
            competitorUrl, productId);

        await _productRepository.RemoveCompetitorAsync(productId, competitorUrl);

        _logger.LogInformation("Competitor with URL {CompetitorUrl} removed successfully from product {ProductId}",
            competitorUrl, productId);

    }

    public async Task<List<CompetitorDTO>> GetCompetitorsAsync(string productId)
    {

        _logger.LogInformation("Getting competitors for product {ProductId}", productId);

        var competitors = await _productRepository.GetCompetitorsAsync(productId);
        var competitorDTOs = _mapper.Map<List<CompetitorDTO>>(competitors);

        _logger.LogInformation("Retrieved {Count} competitors for product {ProductId}",
            competitorDTOs.Count, productId);

        return competitorDTOs;

    }
}
