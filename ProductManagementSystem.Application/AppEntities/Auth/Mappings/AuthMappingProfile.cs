using AutoMapper;
using ProductManagementSystem.Application.AppEntities.Auth.Models;
using ProductManagementSystem.Application.AppEntities.Auth.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Auth.Enum;

namespace ProductManagementSystem.Application.AppEntities.Auth.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<AuthToken, TokenDTO>()
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TokenType == TokenType.Access ? "Bearer" : src.TokenType.ToString().ToLower()))
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.ExpiresAt))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}