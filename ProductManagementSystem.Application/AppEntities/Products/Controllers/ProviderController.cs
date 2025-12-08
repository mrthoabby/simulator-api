using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Products.Services;
using ProductManagementSystem.Application.Common.Errors;
using AutoMapper;
using ProductManagementSystem.Application.Common.AppEntities.Errors;

namespace ProductManagementSystem.Application.AppEntities.Products.Controllers;

[ApiController]
[Route("api/products/{productId}/providers")]
// [Authorize]
public class ProviderController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProviderController> _logger;

    public ProviderController(IProductService productService, IMapper mapper, ILogger<ProviderController> logger)
    {
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProviderDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProviderDTO>> AddProvider(string productId, [FromBody] AddProviderDTO request)
    {
        _logger.LogInformation("Adding provider to product: {ProductId}", productId);
        var provider = await _productService.AddProviderAsync(productId, request);
        var response = _mapper.Map<ProviderDTO>(provider);
        return CreatedAtAction(nameof(GetProviders), new { productId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProviderDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProviderDTO>>> GetProviders(string productId)
    {
        _logger.LogInformation("Getting providers for product: {ProductId}", productId);
        var providers = await _productService.GetProvidersAsync(productId);
        var responses = _mapper.Map<List<ProviderDTO>>(providers);
        return Ok(responses);
    }

    [HttpDelete("{providerId}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveProvider(string productId, string providerId)
    {
        _logger.LogInformation("Removing provider {ProviderId} from product: {ProductId}", providerId, productId);
        await _productService.RemoveProviderAsync(productId, providerId);
        return NoContent();
    }
}