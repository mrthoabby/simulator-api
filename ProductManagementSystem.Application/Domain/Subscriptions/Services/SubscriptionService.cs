
using ProductManagementSystem.Application.Domain.Subscriptions.Repository;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;
using ProductManagementSystem.Application.Domain.Users.Services;
using ProductManagementSystem.Application.Domain.Users.DTOs.Inputs;
using AutoMapper;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(ISubscriptionRepository repository, IMapper mapper, IUserService userService, ILogger<SubscriptionService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<SubscriptionDTO> CreateAsync(CreateSubscriptionDTO subscriptionDto)
    {
        var existingSubscription = await _repository.GetByNameAsync(subscriptionDto.Name);
        if (existingSubscription != null)
        {
            _logger.LogWarning("Subscription creation failed: Subscription with name {SubscriptionName} already exists", subscriptionDto.Name);
            throw new InvalidOperationException("A subscription with this name already exists");
        }

        var price = new Price(subscriptionDto.Price, subscriptionDto.Currency);

        var restrictions = Restrictions.Create(
            subscriptionDto.MaxProducts,
            subscriptionDto.MaxUsers,
            subscriptionDto.MaxCompetitors,
            subscriptionDto.MaxCustomDeductions,
            subscriptionDto.MaxSimulations,
            subscriptionDto.IsPDFExportSupported,
            subscriptionDto.IsSimulationComparisonSupported,
            subscriptionDto.IsExcelExportSupported
        );

        // Crear Subscription usando factory method
        var subscription = Subscription.Create(
            subscriptionDto.Name,
            subscriptionDto.Description,
            price,
            subscriptionDto.Period,
            restrictions
        );

        var savedSubscription = await _repository.CreateAsync(subscription);

        return _mapper.Map<SubscriptionDTO>(savedSubscription);
    }

    public async Task<SubscriptionDTO?> GetByIdAsync(string id)
    {
        var subscription = await _repository.GetByIdAsync(id);
        return subscription != null ? _mapper.Map<SubscriptionDTO>(subscription) : null;
    }

    public async Task<SubscriptionDTO?> GetByNameAsync(string name)
    {
        var subscription = await _repository.GetByNameAsync(name);
        return subscription != null ? _mapper.Map<SubscriptionDTO>(subscription) : null;
    }

    public async Task<PaginatedResult<SubscriptionDTO>> GetAllAsync(SubscriptionFilterDTO filter)
    {
        var paginatedSubscriptions = await _repository.GetAllAsync(filter);

        var subscriptionDTOs = _mapper.Map<List<SubscriptionDTO>>(paginatedSubscriptions.Items);

        return new PaginatedResult<SubscriptionDTO>
        {
            Items = subscriptionDTOs,
            TotalCount = paginatedSubscriptions.TotalCount,
            Page = paginatedSubscriptions.Page,
            PageSize = paginatedSubscriptions.PageSize,
            TotalPages = paginatedSubscriptions.TotalPages,
            HasNextPage = paginatedSubscriptions.HasNextPage,
            HasPreviousPage = paginatedSubscriptions.HasPreviousPage
        };
    }

    public async Task DeleteAsync(string id)
    {
        var subscription = await _repository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new ArgumentException("Subscription not found");
        }
        var activeUsersFilter = new UserFilterDTO
        {
            SubscriptionId = id,
            PageSize = 1
        };
        var activeUsers = await _userService.GetAllNoPaginationAsync(activeUsersFilter);
        if (activeUsers.Any())
        {
            _logger.LogWarning("Subscription deletion failed: Subscription {SubscriptionName} (ID: {SubscriptionId}) has {UserCount} active users",
                subscription.Name, id, activeUsers.Count);
            throw new InvalidOperationException($"Cannot delete subscription '{subscription.Name}' because it has {activeUsers.Count} active users associated with it");
        }

        await _repository.DeleteAsync(id);
    }
}
