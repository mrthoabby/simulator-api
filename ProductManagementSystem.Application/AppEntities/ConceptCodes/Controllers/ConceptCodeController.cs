using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Services;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.Common.AppEntities.Errors;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;

namespace ProductManagementSystem.Application.AppEntities.ConceptCodes.Controllers;

[ApiController]
[Route("api/concept-codes")]
// [Authenticate]
public class ConceptCodeController : ControllerBase
{
    private readonly IConceptCodeService _conceptCodeService;

    public ConceptCodeController(IConceptCodeService conceptCodeService)
    {
        _conceptCodeService = conceptCodeService;
    }

    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ConceptCodeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConceptCodeDTO>> Get([FromRoute] string code)
    {
        var conceptCode = await _conceptCodeService.GetByCodeAsync(code);
        return Ok(conceptCode);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ConceptCodeDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ConceptCodeDTO>>> GetAll([FromQuery] PaginationConfigDTO paginationConfigs, [FromQuery] FilterConceptCodeDTO filter)
    {

        var conceptCodes = await _conceptCodeService.GetAllAsync(paginationConfigs, filter);
        return Ok(conceptCodes);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(List<ConceptCodeDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ConceptCodeDTO>>> GetAll()
    {
        var conceptCodes = await _conceptCodeService.GetAllAsync();
        return Ok(conceptCodes);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ConceptCodeDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConceptCodeDTO>> Create([FromBody] CreateConceptCodeDTO request)
    {
        var conceptCode = await _conceptCodeService.CreateAsync(request);

        return CreatedAtAction(nameof(Get), new { code = conceptCode.Code }, conceptCode);
    }

    [HttpPut("{code}")]
    [ProducesResponseType(typeof(ConceptCodeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConceptCodeDTO>> Update([FromRoute] string code, [FromBody] UpdateConceptCodeDTO request)
    {
        var conceptCode = await _conceptCodeService.UpdateAsync(code, request);
        return Ok(conceptCode);
    }
}