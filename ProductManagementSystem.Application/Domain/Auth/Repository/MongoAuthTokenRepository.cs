using MongoDB.Driver;
using ProductManagementSystem.Application.Domain.Auth.Models;
using ProductManagementSystem.Application.Domain.Auth.Enum;

namespace ProductManagementSystem.Application.Domain.Auth.Repository;

public class MongoAuthTokenRepository : IAuthTokenRepository
{
    private readonly IMongoCollection<AuthToken> _collection;

    public MongoAuthTokenRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<AuthToken>("auth_tokens");
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeysDefinition = Builders<AuthToken>.IndexKeys
            .Ascending(x => x.Token);

        var indexOptions = new CreateIndexOptions
        {
            Unique = true,
            Name = "idx_auth_token_unique"
        };

        _collection.Indexes.CreateOneAsync(new CreateIndexModel<AuthToken>(indexKeysDefinition, indexOptions));

        var userIdIndex = Builders<AuthToken>.IndexKeys.Ascending(x => x.UserId);
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<AuthToken>(userIdIndex, new CreateIndexOptions { Name = "idx_auth_token_userid" }));

        var expirationIndex = Builders<AuthToken>.IndexKeys.Ascending(x => x.ExpiresAt);
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<AuthToken>(expirationIndex, new CreateIndexOptions
        {
            Name = "idx_auth_token_expiration",
            ExpireAfter = TimeSpan.Zero
        }));

        var compositeIndex = Builders<AuthToken>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.TokenType);
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<AuthToken>(compositeIndex, new CreateIndexOptions { Name = "idx_auth_token_userid_type" }));
    }

    public async Task<AuthToken> CreateAsync(AuthToken authToken)
    {
        if (authToken == null) throw new ArgumentNullException(nameof(authToken));

        await _collection.InsertOneAsync(authToken);
        return authToken;
    }

    public async Task<AuthToken?> GetByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        return await _collection.Find(x => x.Token == token).FirstOrDefaultAsync();
    }

    public async Task<AuthToken?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<AuthToken>> GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new List<AuthToken>();

        return await _collection.Find(x => x.UserId == userId).ToListAsync();
    }

    public async Task<List<AuthToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new List<AuthToken>();

        var filter = Builders<AuthToken>.Filter.And(
            Builders<AuthToken>.Filter.Eq(x => x.UserId, userId),
            Builders<AuthToken>.Filter.Eq(x => x.IsRevoked, false),
            Builders<AuthToken>.Filter.Gt(x => x.ExpiresAt, DateTime.UtcNow)
        );

        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<AuthToken>> GetRefreshTokensByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new List<AuthToken>();

        var filter = Builders<AuthToken>.Filter.And(
            Builders<AuthToken>.Filter.Eq(x => x.UserId, userId),
            Builders<AuthToken>.Filter.Eq(x => x.TokenType, TokenType.Refresh),
            Builders<AuthToken>.Filter.Eq(x => x.IsRevoked, false),
            Builders<AuthToken>.Filter.Gt(x => x.ExpiresAt, DateTime.UtcNow)
        );

        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<AuthToken> UpdateAsync(AuthToken authToken)
    {
        if (authToken == null) throw new ArgumentNullException(nameof(authToken));

        await _collection.ReplaceOneAsync(x => x.Id == authToken.Id, authToken);
        return authToken;
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be null or empty", nameof(id));

        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        await _collection.DeleteManyAsync(x => x.UserId == userId);
    }

    public async Task RevokeAllTokensByUserIdAsync(string userId, string revokedBy)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(revokedBy)) throw new ArgumentException("Revoked by cannot be null or empty", nameof(revokedBy));

        var filter = Builders<AuthToken>.Filter.And(
            Builders<AuthToken>.Filter.Eq(x => x.UserId, userId),
            Builders<AuthToken>.Filter.Eq(x => x.IsRevoked, false)
        );

        var update = Builders<AuthToken>.Update
            .Set(x => x.IsRevoked, true)
            .Set(x => x.RevokedAt, DateTime.UtcNow)
            .Set(x => x.RevokedBy, revokedBy);

        await _collection.UpdateManyAsync(filter, update);
    }

    public async Task RevokeExpiredTokensAsync()
    {
        var filter = Builders<AuthToken>.Filter.And(
            Builders<AuthToken>.Filter.Lt(x => x.ExpiresAt, DateTime.UtcNow),
            Builders<AuthToken>.Filter.Eq(x => x.IsRevoked, false)
        );

        var update = Builders<AuthToken>.Update
            .Set(x => x.IsRevoked, true)
            .Set(x => x.RevokedAt, DateTime.UtcNow)
            .Set(x => x.RevokedBy, "System - Expired");

        await _collection.UpdateManyAsync(filter, update);
    }
}