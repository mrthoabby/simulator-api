using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Products.Services;
using AutoMapper;
using ProductManagementSystem.Application.Common.Domain.Errors;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.Domain.Products.Controllers;

[ApiController]
[Route("api/products/{productId}/deductions")]
[Authorize]
public class DeductionController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<DeductionController> _logger;

    public DeductionController(IProductService productService, IMapper mapper, ILogger<DeductionController> logger)
    {
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DeductionDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DeductionDTO>> AddDeduction(string productId, [FromBody] AddDeductionDTO request)
    {
        _logger.LogInformation("Adding deduction to product: {ProductId}", productId);
        var deduction = await _productService.AddDeductionAsync(productId, request);
        var response = _mapper.Map<DeductionDTO>(deduction);
        return CreatedAtAction(nameof(GetDeductions), new { productId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DeductionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DeductionDTO>>> GetDeductions(string productId)
    {
        _logger.LogInformation("Getting deductions for product: {ProductId}", productId);
        var deductions = await _productService.GetDeductionsAsync(productId);
        var responses = _mapper.Map<List<DeductionDTO>>(deductions);
        return Ok(responses);
    }

    [HttpDelete("{conceptCode}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveDeduction(string productId, string conceptCode)
    {
        _logger.LogInformation("Removing deduction {ConceptCode} from product: {ProductId}", conceptCode, productId);
        await _productService.RemoveDeductionAsync(productId, conceptCode);
        return NoContent();
    }
}