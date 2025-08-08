using AutoMapper;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.ConceptCodes.Models;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.Mappings;

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