using AutoMapper;
using ProductManagementSystem.Application.AppEntities.Users.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Users.Models;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.UserPlans.Domain;

namespace ProductManagementSystem.Application.AppEntities.Users.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Credential.Email))
            .ForMember(dest => dest.Teams, opt => opt.Ignore());

        CreateMap<Credential, CredentialDTO>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<PaginatedResult<User>, PaginatedResult<UserDTO>>();
    }

    /// <summary>
    /// 🔗 Método personalizado para mapear User + UserPlan → UserDTO
    /// </summary>
    public static UserDTO MapUserWithPlan(User user, List<UserPlan> userPlans)
    {
        return new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Credential.Email,
            Teams = userPlans.Select(up => new CompanyInfoDTO
            {
                Name = up.Company.Name,
                CompanyId = up.Company.Id,
                SubscriptionId = up.Subscription.Id,
                SubscriptionName = up.Subscription.Name,
                UserPlanCondition = up.OwnerEmail == user.Credential.Email
                    ? EnumUserPlanType.Owner
                    : EnumUserPlanType.Member
            }).ToList()
        };
    }
}