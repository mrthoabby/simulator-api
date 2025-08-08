using AutoMapper;
using ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.GlobalParameters.Models;
using ProductManagementSystem.Application.Domain.Shared.Type;
using ProductManagementSystem.Application.Domain.Shared.DTOs;


namespace ProductManagementSystem.Application.Domain.GlobalParameters.Mappings;

public class GlobalParametersMappingProfile : Profile
{
    public GlobalParametersMappingProfile()
    {
        CreateMap<Money, MoneyDTO>();
        CreateMap<MoneyDTO, Money>()
            .ConstructUsing(dto => Money.Create(dto.Value, dto.Currency));

        CreateMap<GlobalParameter, GlobalParameterDTO>();
        CreateMap<AddGlobalParameterDTO, GlobalParameter>()
            .ForMember(dest => dest.ConceptCode, opt => opt.MapFrom(src => src.ConceptCode))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Application, opt => opt.MapFrom(src => src.Application))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage));

        CreateMap<GlobalParameterDTO, GlobalParameter>()
            .ForMember(dest => dest.ConceptCode, opt => opt.MapFrom(src => src.ConceptCode))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Application, opt => opt.MapFrom(src => src.Application))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage));

        CreateMap<Concept, GlobalParameterDTO>()
            .ForMember(dest => dest.ConceptCode, opt => opt.MapFrom(src => src.ConceptCode))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Application, opt => opt.MapFrom(src => src.Application))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage));
    }
}