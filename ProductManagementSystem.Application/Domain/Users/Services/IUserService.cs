using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Domain.Users.Controllers.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Users.Controllers.DTOs.Outputs;

namespace ProductManagementSystem.Application.Domain.Users.Services;

public interface IUserService
{
    Task<UserDTO> CreateAsync(CreateUserDTO userDTO);
    Task<UserDTO?> GetByIdAsync(string id);
    Task<UserDTO?> GetByEmailAsync(string email);
    Task<PaginatedResult<UserDTO>> GetAllAsync(UserFilterDTO filter);
    Task<List<UserDTO>> GetAllNoPaginationAsync();
}