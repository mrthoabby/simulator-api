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

        // Request DTOs to Domain Entities
        CreateMap<CreateSubscriptionDTO, Subscription>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new Price(src.Price, src.Currency)))
            .ForMember(dest => dest.Restrictions, opt => opt.MapFrom(src => Restrictions.Create(
                src.MaxProducts,
                src.MaxUsers,
                src.MaxCompetitors,
                src.MaxCustomDeductions,
                src.MaxSimulations,
                src.IsPDFExportSupported,
                src.IsSimulationComparisonSupported,
                src.IsExcelExportSupported
            )));
    }
}