using AutoMapper;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Models;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using ProductManagementSystem.Application.Common.AppEntities.Type;

namespace ProductManagementSystem.Application.AppEntities.Subscriptions.Mappings;

public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        //.AppEntities.Entities to Response DTOs
        CreateMap<Subscription, SubscriptionDTO>()
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToString()));

        CreateMap<Price, PriceDTO>();
        CreateMap<Restrictions, RestrictionsDTO>();

        // Paginated Results mapping
        CreateMap<PaginatedResult<Subscription>, PaginatedResult<SubscriptionDTO>>();
    }
}