using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductManagementSystem.Application.Common.AppEntities.Errors;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.Services;

namespace ProductManagementSystem.Application.AppEntities.GlobalParameters.Controllers;

[ApiController]
[Route("api/global-parameters")]
[Authorize]
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