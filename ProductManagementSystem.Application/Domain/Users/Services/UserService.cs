using ProductManagementSystem.Application.Domain.Users.Repository;
using ProductManagementSystem.Application.Domain.Users.DTOs.Inputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Users.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Users.Models;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.UserPlans.Domain;
using ProductManagementSystem.Application.Domain.UserPlans.Repository;
using ProductManagementSystem.Application.Domain.Subscriptions.Repository;
using ProductManagementSystem.Application.Domain.UserPlans.Models;
using ProductManagementSystem.Application.Common.Helpers;
using ProductManagementSystem.Application.Domain.Users.Mappings;
using ProductManagementSystem.Application.Common;
using AutoMapper;

namespace ProductManagementSystem.Application.Domain.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPlanRepository _userPlanRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<UserService> _logger;
    private readonly SecuritySettings _securitySettings;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IUserPlanRepository userPlanRepository, ISubscriptionRepository subscriptionRepository, ILogger<UserService> logger, SecuritySettings securitySettings, IMapper mapper)
    {
        _userRepository = userRepository;
        _userPlanRepository = userPlanRepository;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _securitySettings = securitySettings;
        _mapper = mapper;
    }

    public async Task<UserDTO> CreateAsync(CreateUserDTO userDTO)
    {


        var (subscription, allUserPlans) = await ParallelHelper.TryRunTwoParallelWithResults(
            async () =>
            {
                var sub = await _subscriptionRepository.GetByIdAsync(userDTO.SubscriptionId);
                if (sub == null) throw new NotFoundException("Subscription not found");
                return sub;
            },
            async () =>
            {
                return await _userPlanRepository.GetAllWhereIsMemberAsync(userDTO.Credentials.Email);
            },
            async () =>
            {
                var existingUser = await _userRepository.GetByEmailAsync(userDTO.Credentials.Email);
                if (existingUser != null) throw new ConflictException("User with this email already exists");
            }
        );

        var credential = Credential.Create(userDTO.Credentials.Email, userDTO.Credentials.Password, _securitySettings.PasswordPepper);
        var user = User.Create(userDTO.Name, credential);

        var companyName = userDTO.CompanyName ?? userDTO.Name + "-Company";
        var company = Company.Create(companyName);
        var userPlan = UserPlan.Create(subscription, company, user.Credential.Email);

        var (createdUserPlan, userCreated) = await TransactionHelper.CreateParallelAsync(
            firstFlow: async () =>
            {
                await _userPlanRepository.CreateAsync(userPlan);
                allUserPlans.Add(userPlan);
                return userPlan;
            },
            secondFlow: async () => await _userRepository.CreateAsync(user),
            deleteFirst: async (userPlans) => await _userPlanRepository.DeleteAsync(userPlans.Id),
            deleteSecond: async (userCreated) => await _userRepository.DeleteAsync(userCreated.Id),
            logger: _logger
        );

        return UserMappingProfile.MapUserWithPlan(userCreated, allUserPlans);
    }

    public async Task<UserDTO> ActivateAsync(ActivateUserDTO userDTO)
    {
        var allUserPlans = await ParallelHelper.TryRunAllParallelWithResults(
            async () => await _userPlanRepository.GetAllWhereIsMemberAsync(userDTO.Credentials.Email),
            async () =>
            {
                var existingUser = await _userRepository.GetByEmailAsync(userDTO.Credentials.Email);
                if (existingUser != null) throw new ConflictException("User already exists");
            }
        );

        var credential = Credential.Create(userDTO.Credentials.Email, userDTO.Credentials.Password, _securitySettings.PasswordPepper);
        var user = User.Create(userDTO.Name, credential);

        var createdUser = await _userRepository.CreateAsync(user);

        return UserMappingProfile.MapUserWithPlan(createdUser, allUserPlans);
    }

    public async Task<UserDTO?> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        var userPlans = await _userPlanRepository.GetAllWhereExistsAsync(user.Credential.Email);


        return UserMappingProfile.MapUserWithPlan(user, userPlans ?? new List<UserPlan>());
    }



    public async Task<UserDTO?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return null;

        var userPlans = await _userPlanRepository.GetAllWhereExistsAsync(user.Credential.Email);
        if (userPlans == null || !userPlans.Any()) return null;

        return UserMappingProfile.MapUserWithPlan(user, userPlans);
    }

    public async Task<PaginatedResult<UserDTO>> GetAllAsync(UserFilterDTO filter)
    {
        var paginatedUserResult = await _userRepository.GetAllAsync(PaginationConfigs.Create(filter.Page ?? 1, filter.PageSize ?? 10), filter);
        if (filter.WithUserPlans)
        {
            var tasks = paginatedUserResult.Items.Select(async user =>
            {
                var userPlans = await _userPlanRepository.GetAllWhereIsMemberAsync(user.Credential.Email);
                return UserMappingProfile.MapUserWithPlan(user, userPlans);
            });

            var result = await Task.WhenAll(tasks);

            return new PaginatedResult<UserDTO>
            {
                Items = result.ToList(),
                TotalCount = paginatedUserResult.TotalCount,
                Page = paginatedUserResult.Page,
                PageSize = paginatedUserResult.PageSize,
                TotalPages = paginatedUserResult.TotalPages,
                HasNextPage = paginatedUserResult.HasNextPage,
                HasPreviousPage = paginatedUserResult.HasPreviousPage
            };
        }
        else
        {
            return new PaginatedResult<UserDTO>
            {
                Items = paginatedUserResult.Items.Select(user => new UserDTO { Id = user.Id, Name = user.Name, Email = user.Credential.Email, Teams = new List<CompanyInfoDTO>() }).ToList(),
                TotalCount = paginatedUserResult.TotalCount,
                Page = paginatedUserResult.Page,
                PageSize = paginatedUserResult.PageSize,
                TotalPages = paginatedUserResult.TotalPages,
                HasNextPage = paginatedUserResult.HasNextPage,
                HasPreviousPage = paginatedUserResult.HasPreviousPage
            };
        }
    }


    public async Task<List<UserDTO>> GetAllNoPaginationAsync(UserFilterDTO filter)
    {
        var users = await _userRepository.GetAllNoPaginationAsync(filter);
        if (filter.WithUserPlans)
        {
            var tasks = users.Select(async user =>
            {
                var userPlans = await _userPlanRepository.GetAllWhereIsMemberAsync(user.Credential.Email);
                return UserMappingProfile.MapUserWithPlan(user, userPlans);
            });
            return (await Task.WhenAll(tasks)).ToList();
        }
        else
        {
            return _mapper.Map<List<UserDTO>>(users);
        }
    }

}
