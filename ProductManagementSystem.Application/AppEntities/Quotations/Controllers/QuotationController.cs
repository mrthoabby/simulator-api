using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Quotations.Services;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Errors;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Controllers;

[ApiController]
[Route("api/products/{productId}/quotations")]
public class QuotationController : ControllerBase
{
    private readonly IQuotationService _quotationService;
    private readonly ILogger<QuotationController> _logger;

    public QuotationController(IQuotationService quotationService, ILogger<QuotationController> logger)
    {
        _quotationService = quotationService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuotationDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuotationDTO>> Create(string productId, [FromBody] CreateQuotationDTO request)
    {
        _logger.LogInformation("Creating quotation for product {ProductId}", productId);

        // Ensure the productId in the route matches the one in the request body
        if (request.ProductId != productId)
        {
            return BadRequest(ErrorResponse.Create(
                "BadRequest", 
                "Product ID in route does not match Product ID in request body",
                $"Route productId: {productId}, Request productId: {request.ProductId}"
            ));
        }

        var quotation = await _quotationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { productId, id = quotation.Id }, quotation);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuotationDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuotationDTO>> GetById(string productId, string id)
    {
        _logger.LogInformation("Getting quotation {QuotationId} for product {ProductId}", id, productId);

        var quotation = await _quotationService.GetByIdAsync(id);
        if (quotation == null)
        {
            throw new NotFoundException("Quotation not found");
        }

        // Validate that the quotation belongs to the product
        if (quotation.ProductId != productId)
        {
            throw new NotFoundException("Quotation not found for this product");
        }

        return Ok(quotation);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<QuotationDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<QuotationDTO>>> GetByProductId(
        string productId, 
        [FromQuery] PaginationConfigDTO paginationConfig, 
        [FromQuery] FilterQuotationDTO? filter)
    {
        _logger.LogInformation("Getting quotations for product {ProductId}", productId);

        var paginatedQuotations = await _quotationService.GetByProductIdAsync(productId, paginationConfig, filter);
        return Ok(paginatedQuotations);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(List<QuotationDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<QuotationDTO>>> GetAllByProductId(string productId)
    {
        _logger.LogInformation("Getting all quotations for product {ProductId}", productId);

        var quotations = await _quotationService.GetAllByProductIdAsync(productId);
        return Ok(quotations);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(QuotationDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuotationDTO>> Update(string productId, string id, [FromBody] UpdateQuotationDTO request)
    {
        _logger.LogInformation("Updating quotation {QuotationId} for product {ProductId}", id, productId);

        // Validate that the quotation belongs to the product
        var existingQuotation = await _quotationService.GetByIdAsync(id);
        if (existingQuotation == null || existingQuotation.ProductId != productId)
        {
            throw new NotFoundException("Quotation not found for this product");
        }

        var quotation = await _quotationService.UpdateAsync(id, request);
        return Ok(quotation);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(string productId, string id)
    {
        _logger.LogInformation("Deleting quotation {QuotationId} for product {ProductId}", id, productId);

        // Validate that the quotation belongs to the product
        var existingQuotation = await _quotationService.GetByIdAsync(id);
        if (existingQuotation == null || existingQuotation.ProductId != productId)
        {
            throw new NotFoundException("Quotation not found for this product");
        }

        await _quotationService.DeleteAsync(id);
        return NoContent();
    }
}

