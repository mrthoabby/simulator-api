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
[Route("api/products/{productId}/competitors")]
[Authorize]
public class CompetitorController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<CompetitorController> _logger;

    public CompetitorController(IProductService productService, IMapper mapper, ILogger<CompetitorController> logger)
    {
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CompetitorDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CompetitorDTO>> AddCompetitor(string productId, [FromBody] AddCompetitorDTO request)
    {
        _logger.LogInformation("Adding competitor to product: {ProductId}", productId);
        var competitor = await _productService.AddCompetitorAsync(productId, request);
        var response = _mapper.Map<CompetitorDTO>(competitor);
        return CreatedAtAction(nameof(GetCompetitors), new { productId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CompetitorDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CompetitorDTO>>> GetCompetitors(string productId)
    {
        _logger.LogInformation("Getting competitors for product: {ProductId}", productId);
        var competitors = await _productService.GetCompetitorsAsync(productId);
        var responses = _mapper.Map<List<CompetitorDTO>>(competitors);
        return Ok(responses);
    }

    [HttpDelete("{competitorUrl}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveCompetitor(string productId, string competitorUrl)
    {
        _logger.LogInformation("Removing competitor {CompetitorUrl} from product: {ProductId}", competitorUrl, productId);
        await _productService.RemoveCompetitorAsync(productId, competitorUrl);
        return NoContent();
    }
}