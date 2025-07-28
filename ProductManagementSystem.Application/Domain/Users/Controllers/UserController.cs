using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Domain.Users.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Users.Services;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Common.Domain.Errors;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.Domain.Users.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDTO>> Create([FromBody] CreateUserDTO request)
    {
        _logger.LogInformation("Creating user with subscription id: {SubscriptionId}", request.SubscriptionId);
        var user = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPost("activate")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDTO>> Activate([FromBody] ActivateUserDTO request)
    {
        var user = await _userService.ActivateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDTO>> GetById(string id)
    {
        _logger.LogInformation("Getting user by id: {Id}", id);
        var user = await _userService.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User not found");
        return Ok(user);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<UserDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<UserDTO>>> GetAll([FromQuery] UserFilterDTO filter)
    {
        _logger.LogInformation("Getting all users with filter: {Filter}", filter);
        var paginatedUsers = await _userService.GetAllAsync(filter);
        return Ok(paginatedUsers);
    }
}
