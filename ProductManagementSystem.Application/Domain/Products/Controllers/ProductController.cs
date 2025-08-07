using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Domain.Products.Services;
using AutoMapper;
using ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Domain.Errors;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Domain.Products.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authenticate]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, IMapper mapper, ILogger<ProductController> logger)
    {
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDTO>> Create([FromBody] CreateProductDTO request)
    {
        _logger.LogInformation("Creating product: {Name}", request.Name);
        var product = await _productService.CreateAsync(request);
        var response = _mapper.Map<ProductDTO>(product);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDTO>> GetById(string id)
    {
        _logger.LogInformation("Getting product by id: {Id}", id);
        var product = await _productService.GetByIdAsync(id);
        if (product == null) throw new NotFoundException("Product not found");
        var response = _mapper.Map<ProductDTO>(product);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<ProductDTO>>> GetAll([FromQuery] PaginationConfigDTO paginationConfig, [FromQuery] FilterProductDTO? filter, [FromQuery] string? search)
    {
        _logger.LogInformation("Getting all products with filter: {Filter}, Search: {Search}", filter, search);
        var paginatedProducts = await _productService.GetAllAsync(PaginationConfigs.Create(paginationConfig.Page, paginationConfig.PageSize), filter, search);
        return Ok(paginatedProducts);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDTO>> Update(string id, [FromBody] UpdateProductDTO request)
    {
        _logger.LogInformation("Updating product: {Id}", id);
        var product = await _productService.UpdateAsync(id, request);
        var response = _mapper.Map<ProductDTO>(product);
        return Ok(response);
    }

    [HttpPatch("{id}/image")]
    [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDTO>> UpdateImage(string id, [FromBody] UpdateProductImageDTO request)
    {
        _logger.LogInformation("Updating product image: {Id}", id);
        var product = await _productService.UpdateImageAsync(id, request);
        var response = _mapper.Map<ProductDTO>(product);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting product: {Id}", id);
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
