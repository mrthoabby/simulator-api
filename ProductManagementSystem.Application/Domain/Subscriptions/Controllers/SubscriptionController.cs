using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Domain.Subscriptions.Services;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using AutoMapper;
using FluentValidation;
using ProductManagementSystem.Application.Domain.Users.Services;
using ProductManagementSystem.Application.Domain.Users.DTOs.Inputs;
using ProductManagementSystem.Application.Common.Domain.Errors;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionController> _logger;
    private readonly IUserService _userService;

    public SubscriptionController(ISubscriptionService subscriptionService, IMapper mapper, ILogger<SubscriptionController> logger, IUserService userService)
    {
        _subscriptionService = subscriptionService;
        _mapper = mapper;
        _logger = logger;
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionDTO>> Create([FromBody] CreateSubscriptionDTO request)
    {

        var subscription = await _subscriptionService.CreateAsync(request);
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = subscription.Id },
            value: subscription
        );

    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SubscriptionDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubscriptionDTO>> GetById(string id)
    {
        var subscription = await _subscriptionService.GetByIdAsync(id);
        if (subscription == null)
        {
            return NotFound($"Subscription with ID {id} not found");
        }
        return Ok(subscription);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<SubscriptionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<SubscriptionDTO>>> GetAll([FromQuery] SubscriptionFilterDTO filter)
    {
        var paginatedSubscriptions = await _subscriptionService.GetAllAsync(filter);
        return Ok(paginatedSubscriptions);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(string id)
    {

        await _subscriptionService.DeleteAsync(id);
        return Ok(new { message = "Subscription deleted successfully" });

    }
}