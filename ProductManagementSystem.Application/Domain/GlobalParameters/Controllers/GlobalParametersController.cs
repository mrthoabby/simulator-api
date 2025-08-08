using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Common.Domain.Errors;
using ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.GlobalParameters.Services;

namespace ProductManagementSystem.Application.Domain.GlobalParameters.Controllers;

[ApiController]
[Route("api/[controller]")]

public class GlobalParametersController : ControllerBase
{
    private readonly IGlobalParametersService _service;

    public GlobalParametersController(
        IGlobalParametersService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GlobalParameterDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<GlobalParameterDTO>>> Get()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }


    [HttpPost("add")]
    [ProducesResponseType(typeof(GlobalParameterDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GlobalParameterDTO>> AddGlobalParameter([FromBody] AddGlobalParameterDTO addDTO)
    {
        var result = await _service.CreateAsync(addDTO);
        return Ok(result);
    }

    [HttpPut("{conceptCode}")]
    [ProducesResponseType(typeof(GlobalParameterDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GlobalParameterDTO>> UpdateGlobalParameter([FromRoute] string conceptCode, [FromBody] UpdateGlobalParameterDTO updateDTO)
    {
        var result = await _service.UpdateAsync(conceptCode, updateDTO);
        return Ok(result);
    }
}