using AutoMapper;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Models;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Type;

namespace ProductManagementSystem.Application.AppEntities.ConceptCodes.Mappings;

public class ConceptCodeMappingProfile : Profile
{
    public ConceptCodeMappingProfile()
    {
        CreateMap<ConceptCode, ConceptCodeDTO>();
        CreateMap<CreateConceptCodeDTO, ConceptCode>();
        CreateMap<UpdateConceptCodeDTO, ConceptCode>();

        CreateMap<ConceptCode, string>().ConvertUsing(src => src.Code);
        CreateMap<string, ConceptCode>().ConvertUsing(src => ConceptCode.Create(src, false));

        CreateMap<PaginationConfigDTO, PaginationConfigs>()
            .ConvertUsing(src => PaginationConfigs.Create(src.Page, src.PageSize));

        CreateMap<PaginatedResult<ConceptCode>, PaginatedResult<ConceptCodeDTO>>();
    }
}