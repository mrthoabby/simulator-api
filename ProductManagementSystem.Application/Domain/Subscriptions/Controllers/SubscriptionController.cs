using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Domain.Subscriptions.Services;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Requests;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Responses;
using ProductManagementSystem.Application.Common.Domain.Type;
using AutoMapper;
using FluentValidation;
using ProductManagementSystem.Application.Domain.Users.Services;

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
    public async Task<ActionResult<SubscriptionDTO>> Create([FromBody] CreateSubscriptionDTO request)
    {
        try
        {
            var subscription = await _subscriptionService.CreateAsync(request);
            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = subscription.Id },
                value: subscription
            );
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating subscription: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating subscription");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while processing your request" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionDTO>> GetById(string id)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(id);
            if (subscription == null)
            {
                return NotFound($"Subscription with ID {id} not found");
            }
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while processing your request" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<SubscriptionDTO>>> GetAll([FromQuery] SubscriptionFilterDTO? filter)
    {
        try
        {
            filter ??= new SubscriptionFilterDTO { Page = 1, PageSize = 10 };

            var paginatedSubscriptions = await _subscriptionService.GetAllAsync(filter);
            return Ok(paginatedSubscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscriptions list");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while processing your request" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var allUsers = await _userService.GetAllNoPaginationAsync();
            if (allUsers.Any(x => x.SubscriptionId == id))
            {
                return BadRequest("Cannot delete subscription with active users");
            }

            await _subscriptionService.DeleteAsync(id);
            return Ok(new { message = "Subscription deleted successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Subscription with ID {Id} not found for deletion", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while processing your request" });
        }
    }
}