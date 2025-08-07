using AutoMapper;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.Models;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.Mappings;

public class DeductionCodeMappingProfile : Profile
{
    public DeductionCodeMappingProfile()
    {
        CreateMap<DeductionCode, DeductionCodeDTO>();
        CreateMap<CreateDeductionCodeDTO, DeductionCode>()
            .ConstructUsing(src => DeductionCode.Create(src.Code, src.IsFromSystem ?? false));
        CreateMap<UpdateDeductionCodeDTO, DeductionCode>()
            .ConstructUsing(src => DeductionCode.Create(src.Code, false));

        CreateMap<DeductionCode, string>().ConvertUsing(src => src.Code);
        CreateMap<string, DeductionCode>().ConvertUsing(src => DeductionCode.Create(src, false));
    }
}