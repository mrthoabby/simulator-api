using AutoMapper;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Quotations.Models;
using ProductManagementSystem.Application.AppEntities.Quotations.Repository;
using ProductManagementSystem.Application.AppEntities.Products.Repository;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Services;

public class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<QuotationService> _logger;

    public QuotationService(
        IQuotationRepository quotationRepository,
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<QuotationService> logger)
    {
        _quotationRepository = quotationRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<QuotationDTO> CreateAsync(CreateQuotationDTO dto)
    {
        _logger.LogInformation("Creating quotation for product {ProductId} with provider {ProviderId}", 
            dto.ProductId, dto.ProviderId);

        // Validate that the product exists
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {dto.ProductId} not found");
        }

        // Validate that the provider exists in the product
        var provider = product.Providers?.FirstOrDefault(p => p.Id == dto.ProviderId);
        if (provider == null)
        {
            throw new NotFoundException($"Provider with ID {dto.ProviderId} not found in product {dto.ProductId}");
        }

        var dimensions = Dimensions.Create(dto.Dimensions.Width, dto.Dimensions.Height, dto.Dimensions.Depth);
        var quotation = Quotation.Create(
            dto.ProductId, 
            dto.ProviderId, 
            provider.Name, 
            dimensions, 
            dto.UnitsPerBox, 
            dto.TotalUnits, 
            dto.IsActive
        );

        var createdQuotation = await _quotationRepository.CreateAsync(quotation);
        var quotationDto = _mapper.Map<QuotationDTO>(createdQuotation);

        _logger.LogInformation("Quotation created successfully with ID: {QuotationId}", createdQuotation.Id);
        return quotationDto;
    }

    public async Task<QuotationDTO?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Getting quotation by ID: {QuotationId}", id);

        var quotation = await _quotationRepository.GetByIdAsync(id);
        if (quotation == null)
        {
            _logger.LogWarning("Quotation not found with ID: {QuotationId}", id);
            return null;
        }

        var quotationDto = _mapper.Map<QuotationDTO>(quotation);
        return quotationDto;
    }

    public async Task<PaginatedResult<QuotationDTO>> GetByProductIdAsync(string productId, PaginationConfigDTO paginationConfigs, FilterQuotationDTO? filter = null)
    {
        _logger.LogInformation("Getting quotations for product {ProductId} with pagination: Page {Page}, PageSize {PageSize}",
            productId, paginationConfigs.Page, paginationConfigs.PageSize);

        // Validate that the product exists
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {productId} not found");
        }

        var paginationConfig = _mapper.Map<PaginationConfigs>(paginationConfigs);
        var paginatedQuotations = await _quotationRepository.GetByProductIdAsync(productId, paginationConfig, filter);

        var quotationsDTO = _mapper.Map<List<QuotationDTO>>(paginatedQuotations.Items);

        var result = new PaginatedResult<QuotationDTO>
        {
            Items = quotationsDTO,
            TotalCount = paginatedQuotations.TotalCount,
            Page = paginatedQuotations.Page,
            PageSize = paginatedQuotations.PageSize,
            TotalPages = paginatedQuotations.TotalPages,
            HasNextPage = paginatedQuotations.HasNextPage,
            HasPreviousPage = paginatedQuotations.HasPreviousPage
        };

        _logger.LogInformation("Retrieved {Count} quotations out of {TotalCount} total for product {ProductId}",
            quotationsDTO.Count, paginatedQuotations.TotalCount, productId);

        return result;
    }

    public async Task<List<QuotationDTO>> GetAllByProductIdAsync(string productId)
    {
        _logger.LogInformation("Getting all quotations for product {ProductId}", productId);

        // Validate that the product exists
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {productId} not found");
        }

        var quotations = await _quotationRepository.GetAllByProductIdAsync(productId);
        var quotationsDTO = _mapper.Map<List<QuotationDTO>>(quotations);

        _logger.LogInformation("Retrieved {Count} quotations for product {ProductId}", quotationsDTO.Count, productId);

        return quotationsDTO;
    }

    public async Task<QuotationDTO> UpdateAsync(string id, UpdateQuotationDTO dto)
    {
        _logger.LogInformation("Updating quotation with ID: {QuotationId}", id);

        var existingQuotation = await _quotationRepository.GetByIdAsync(id);
        if (existingQuotation == null)
        {
            throw new NotFoundException($"Quotation with ID {id} not found");
        }

        Dimensions? dimensions = null;
        if (dto.Dimensions != null)
        {
            dimensions = Dimensions.Create(dto.Dimensions.Width, dto.Dimensions.Height, dto.Dimensions.Depth);
        }

        existingQuotation.Update(dimensions, dto.UnitsPerBox, dto.TotalUnits, dto.IsActive);

        var updatedQuotation = await _quotationRepository.UpdateAsync(id, existingQuotation);
        var quotationDto = _mapper.Map<QuotationDTO>(updatedQuotation);

        _logger.LogInformation("Quotation updated successfully with ID: {QuotationId}", id);
        return quotationDto;
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting quotation with ID: {QuotationId}", id);

        await _quotationRepository.DeleteAsync(id);

        _logger.LogInformation("Quotation deleted successfully with ID: {QuotationId}", id);
    }

    public async Task DeleteByProductIdAsync(string productId)
    {
        _logger.LogInformation("Deleting all quotations for product {ProductId}", productId);

        await _quotationRepository.DeleteByProductIdAsync(productId);

        _logger.LogInformation("All quotations deleted for product {ProductId}", productId);
    }
}

