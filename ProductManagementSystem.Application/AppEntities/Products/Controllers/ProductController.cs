using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.AppEntities.Products.Services;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Common.AppEntities.Errors;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Type;

namespace ProductManagementSystem.Application.AppEntities.Products.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService productService, ILogger<ProductController> logger) : ControllerBase
{
    private readonly IProductService _productService = productService;
    private readonly ILogger<ProductController> _logger = logger;

    [HttpPost]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDTO>> Create([FromBody] CreateProductDTO request)
    {
        _logger.LogInformation("Creating product: {Name}", request.Name);
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDTO>> GetById(string id)
    {
        _logger.LogInformation("Getting product by id: {Id}", id);
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }
        return Ok(product);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<ProductDTO>>> GetAll([FromQuery] PaginationConfigDTO paginationConfig, [FromQuery] FilterProductDTO? filter, [FromQuery] string? search)
    {
        _logger.LogInformation("Getting all products with pagination: {PaginationConfig}, filter: {Filter}, query: {Query}", paginationConfig, filter, search);
        var paginatedProducts = await _productService.GetAllAsync(paginationConfig, filter, search);
        return Ok(paginatedProducts);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDTO>> Update(string id, [FromBody] UpdateProductDTO request)
    {
        _logger.LogInformation("Updating product: {Id}", id);
        var product = await _productService.UpdateAsync(id, request);
        return Ok(product);
    }

    [HttpPatch("{id}/image")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDTO>> UpdateImage(string id, [FromBody] UpdateProductImageDTO request)
    {
        _logger.LogInformation("Updating product image: {Id}", id);
        var product = await _productService.UpdateImageAsync(id, request);
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting product: {Id}", id);
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
