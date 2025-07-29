using ProductManagementSystem.Application.Domain.Auth.Models;

namespace ProductManagementSystem.Application.Domain.Auth.Repository;

public interface IAuthTokenRepository
{
    Task<AuthToken> CreateAsync(AuthToken authToken);
    Task<AuthToken> UpdateAsync(AuthToken authToken);
    Task<AuthToken?> GetByTokenAsync(string token);
    Task<AuthToken?> GetByIdAsync(string id);
    Task RevokeAllTokensByUserIdAsync(string userId, string revokedBy);
    Task RevokeExpiredTokensAsync();
    Task DeleteAsync(string id);

    Task<List<AuthToken>> GetActiveTokensByUserIdAsync(string userId);
}