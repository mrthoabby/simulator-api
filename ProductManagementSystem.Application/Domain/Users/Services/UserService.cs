using ProductManagementSystem.Application.Domain.Users.Repository;
using ProductManagementSystem.Application.Domain.Users.Controllers.DTOs.Inputs;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Users.Controllers.DTOs.Outputs;
using AutoMapper;
using ProductManagementSystem.Application.Domain.Users.Models;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.UserPlans.Domain;
using ProductManagementSystem.Application.Domain.UserPlans.Repository;
using ProductManagementSystem.Application.Domain.Subscriptions.Repository;
using ProductManagementSystem.Application.Domain.UserPlans.Models;

namespace ProductManagementSystem.Application.Domain.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IUserPlanRepository _userPlanRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public UserService(IUserRepository userRepository, IMapper mapper, IUserPlanRepository userPlanRepository, ISubscriptionRepository subscriptionRepository)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userPlanRepository = userPlanRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<UserDTO> CreateAsync(CreateUserDTO userDTO)
    {
        var allUsers = await _userRepository.GetAllNoPaginationAsync();
        if (allUsers.Any(x => x.Credential.Email == userDTO.Credentials.Email))
        {
            throw new InvalidOperationException("User already registered");
        }

        var subscription = await _subscriptionRepository.GetByIdAsync(userDTO.SubscriptionId);
        if (subscription == null)
        {
            throw new NotFoundException("Subscription not found");
        }


        var credential = Credential.Create(userDTO.Credentials.Email, userDTO.Credentials.Password);
        var user = User.Create(userDTO.Name, credential);


        var company = Company.Create(userDTO.CompanyName ?? userDTO.Name + "-Company");
        var userPlan = UserPlan.Create(subscription, company, user.Id);
        var userPlanCreated = await _userPlanRepository.CreateAsync(userPlan);

        user.SetUserPlan(userPlan);

        var userCreated = await _userRepository.SaveAsync(user);
        return _mapper.Map<UserDTO>(userCreated);
    }

    public async Task<UserDTO?> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return null;
        }
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<PaginatedResult<UserDTO>> GetAllAsync(UserFilterDTO filter)
    {
        var users = await _userRepository.GetAllAsync(filter);
        return _mapper.Map<PaginatedResult<UserDTO>>(users);
    }

    public async Task<List<UserDTO>> GetAllNoPaginationAsync()
    {
        var users = await _userRepository.GetAllNoPaginationAsync();
        return _mapper.Map<List<UserDTO>>(users);
    }

}
