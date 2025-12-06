using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Users.Models;

namespace ProductManagementSystem.Application.AppEntities.Users.Repository;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<PaginatedResult<User>> GetAllAsync(PaginationConfigs paginationConfigs, IUserFilters userFilters);
    Task<List<User>> GetAllNoPaginationAsync(IUserFilters userFilters);
    Task DeleteAsync(string id);
}
