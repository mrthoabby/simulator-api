using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Domain.DeductionCodes.Services;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Attributes;
using ProductManagementSystem.Application.Common.Errors;
using AutoMapper;
using ProductManagementSystem.Application.Common.Domain.Errors;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authenticate]
public class DeductionCodeController : ControllerBase
{
    private readonly IDeductionCodeService _deductionCodeService;
    private readonly IMapper _mapper;
    private readonly ILogger<DeductionCodeController> _logger;

    public DeductionCodeController(IDeductionCodeService deductionCodeService, IMapper mapper, ILogger<DeductionCodeController> logger)
    {
        _deductionCodeService = deductionCodeService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DeductionCodeDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DeductionCodeDTO>>> GetAll()
    {
        _logger.LogInformation("Getting all deduction codes");

        var deductionCodes = await _deductionCodeService.GetAllAsync();
        var response = _mapper.Map<List<DeductionCodeDTO>>(deductionCodes);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DeductionCodeDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DeductionCodeDTO>> Create([FromBody] CreateDeductionCodeDTO request)
    {
        _logger.LogInformation("Creating deduction code: {Code}", request.Code);

        var deductionCode = await _deductionCodeService.CreateAsync(request);
        var response = _mapper.Map<DeductionCodeDTO>(deductionCode);

        return CreatedAtAction(nameof(GetAll), response);
    }
}