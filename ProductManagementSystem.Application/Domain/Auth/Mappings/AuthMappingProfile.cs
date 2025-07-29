using AutoMapper;
using ProductManagementSystem.Application.Domain.Auth.Models;
using ProductManagementSystem.Application.Domain.Auth.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Auth.Enum;

namespace ProductManagementSystem.Application.Domain.Auth.Mappings;

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