using AutoMapper;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;
using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Domain.Subscriptions.Mappings;

public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        // Domain Entities to Response DTOs
        CreateMap<Subscription, SubscriptionDTO>()
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToString()));

        CreateMap<Price, PriceDTO>();
        CreateMap<Restrictions, RestrictionsDTO>();

        // Paginated Results mapping
        CreateMap<PaginatedResult<Subscription>, PaginatedResult<SubscriptionDTO>>();
    }
}