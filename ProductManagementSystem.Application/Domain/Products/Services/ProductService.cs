using ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Products.Repository;
using ProductManagementSystem.Application.Domain.Products.Models;
using ProductManagementSystem.Application.Domain.Shared.Type;
using ProductManagementSystem.Application.Domain.Shared.Enum;
using AutoMapper;
using ProductManagementSystem.Application.Common.Errors;


namespace ProductManagementSystem.Application.Domain.Products.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IDeductionDomainRules _deductionDomainRules;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, IDeductionDomainRules deductionDomainRules, IMapper mapper, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _deductionDomainRules = deductionDomainRules;
        _mapper = mapper;
        _logger = logger;
    }



    public async Task<ProductDTO> CreateAsync(CreateProductDTO request)
    {
        _logger.LogInformation("Creating product: {Name}", request.Name);
        var price = Money.Create(request.Price.Value, request.Price.Currency);
        var productBuilder = Product.Create(request.Name, price);
        if (!string.IsNullOrEmpty(request.ImageUrl))
            productBuilder.WithImageUrl(request.ImageUrl);

        if (request.Deductions != null && request.Deductions.Any())
        {
            var deductions = _mapper.Map<List<Deduction>>(request.Deductions);
            await _deductionDomainRules.Validate(deductions);
            productBuilder.WithDeductions(deductions);
        }

        if (request.Providers != null && request.Providers.Any())
            productBuilder
            .WithProviders(_mapper
            .Map<List<Provider>>(request.Providers));

        if (request.Competitors != null && request.Competitors.Any())
            productBuilder
            .WithCompetitors(_mapper
            .Map<List<Competitor>>(request.Competitors));

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

    public async Task<PaginatedResult<ProductDTO>> GetAllAsync(PaginationConfigs paginationConfigs, FilterProductDTO? filter = null, string? search = null)
    {
        try
        {
            _logger.LogInformation("Getting all products with pagination: Page {Page}, PageSize {PageSize}",
                paginationConfigs.Page, paginationConfigs.PageSize);

            var paginatedProducts = await _productRepository.GetAllAsync(paginationConfigs, filter, search);

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

    public async Task<ProductDTO> UpdateAsync(string id, UpdateProductDTO request)
    {

        _logger.LogInformation("Updating product with ID: {ProductId}", id);

        // Obtener el producto existente
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        // Actualizar propiedades
        var productBuilder = Product.Create(existingProduct.Name, existingProduct.Price);
        if (!string.IsNullOrEmpty(request.Name))
            productBuilder.WithName(request.Name);

        if (request.Price != null)
        {
            var currency = Enum.Parse<EnumCurrency>(request.Price.Currency.ToString());
            productBuilder.WithPrice(Money.Create(request.Price.Value, currency));
        }

        if (!string.IsNullOrEmpty(request.ImageUrl))
            productBuilder.WithImageUrl(request.ImageUrl);

        // Validar y actualizar
        var validatedProduct = productBuilder.Build();
        var updatedProduct = await _productRepository.UpdateAsync(id, validatedProduct);

        var productDto = _mapper.Map<ProductDTO>(updatedProduct);

        _logger.LogInformation("Product updated successfully: {ProductId}", id);
        return productDto;

    }

    public async Task<ProductDTO> UpdateImageAsync(string id, UpdateProductImageDTO request)
    {

        _logger.LogInformation("Updating image for product with ID: {ProductId}", id);

        // Obtener el producto existente
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        // Actualizar solo la imagen
        var productBuilder = Product.Create(existingProduct.Name, existingProduct.Price);
        if (!string.IsNullOrEmpty(request.ImageUrl))
            productBuilder.WithImageUrl(request.ImageUrl);

        // Validar y actualizar
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

    // Deduction operations
    public Task<DeductionDTO> AddDeductionAsync(string productId, AddDeductionDTO request)
    {

        throw new NotImplementedException();

    }

    public async Task RemoveDeductionAsync(string productId, string conceptCode)
    {

        _logger.LogInformation("Removing deduction {ConceptCode} from product {ProductId}",
            conceptCode, productId);

        await _productRepository.RemoveDeductionAsync(productId, conceptCode);

        _logger.LogInformation("Deduction {ConceptCode} removed successfully from product {ProductId}",
        conceptCode, productId);

    }

    public async Task<List<DeductionDTO>> GetDeductionsAsync(string productId)
    {

        _logger.LogInformation("Getting deductions for product {ProductId}", productId);

        var deductions = await _productRepository.GetDeductionsAsync(productId);
        var deductionDtos = _mapper.Map<List<DeductionDTO>>(deductions);

        _logger.LogInformation("Retrieved {Count} deductions for product {ProductId}",
            deductionDtos.Count, productId);

        return deductionDtos;

    }

    // Provider operations
    public async Task<ProviderDTO> AddProviderAsync(string productId, AddProviderDTO request)
    {

        _logger.LogInformation("Adding provider {ProviderName} to product {ProductId}",
            request.Name, productId);

        var provider = Provider.Create(request.Name, request.Url, new List<Offer>());

        // Agregar el proveedor al producto
        var addedProvider = await _productRepository.AddProviderAsync(productId, provider);

        var providerDto = _mapper.Map<ProviderDTO>(addedProvider);

        _logger.LogInformation("Provider {ProviderName} added successfully to product {ProductId}",
            request.Name, productId);

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
    public async Task<CompetitorDTO> AddCompetitorAsync(string productId, AddCompetitorDTO request)
    {

        _logger.LogInformation("Adding competitor with URL {CompetitorUrl} to product {ProductId}",
            request.Url, productId);

        var price = Money.Create(request.Price.Value, request.Price.Currency);
        var competitor = Competitor.Create(request.Name, price, request.Url, request.ImageUrl);

        // Agregar el competidor al producto
        var addedCompetitor = await _productRepository.AddCompetitorAsync(productId, competitor);

        var competitorDto = _mapper.Map<CompetitorDTO>(addedCompetitor);

        _logger.LogInformation("Competitor with URL {CompetitorUrl} added successfully to product {ProductId}",
            request.Url, productId);

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
        var competitorDtos = _mapper.Map<List<CompetitorDTO>>(competitors);

        _logger.LogInformation("Retrieved {Count} competitors for product {ProductId}",
            competitorDtos.Count, productId);

        return competitorDtos;

    }
}
