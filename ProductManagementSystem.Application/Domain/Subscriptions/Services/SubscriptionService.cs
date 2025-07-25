
using ProductManagementSystem.Application.Domain.Subscriptions.Repository;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Requests;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Responses;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;
using AutoMapper;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly IMapper _mapper;

    public SubscriptionService(ISubscriptionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SubscriptionDTO> CreateAsync(CreateSubscriptionDTO subscriptionDto)
    {
        var existingSubscription = await _repository.GetByNameAsync(subscriptionDto.Name);
        if (existingSubscription != null)
        {
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

        await _repository.DeleteAsync(id);
    }
}
