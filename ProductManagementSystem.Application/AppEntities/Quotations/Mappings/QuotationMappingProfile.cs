using AutoMapper;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Quotations.Models;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Mappings;

public class QuotationMappingProfile : Profile
{
    public QuotationMappingProfile()
    {
        // Quotation -> QuotationDTO
        CreateMap<Quotation, QuotationDTO>()
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => new DimensionsDTO
            {
                Width = src.Dimensions.Width,
                Height = src.Dimensions.Height,
                Depth = src.Dimensions.Depth
            }));

        // Dimensions -> DimensionsDTO
        CreateMap<Dimensions, DimensionsDTO>();
        
        // DimensionsDTO -> Dimensions
        CreateMap<DimensionsDTO, Dimensions>()
            .ConstructUsing(dto => Dimensions.Create(dto.Width, dto.Height, dto.Depth));
    }
}

