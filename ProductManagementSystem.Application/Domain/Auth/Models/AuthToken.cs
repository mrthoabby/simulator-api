using FluentValidation;
using ProductManagementSystem.Application.Domain.Auth.Enum;

namespace ProductManagementSystem.Application.Domain.Auth.Models;

public class AuthToken
{
    public string Id { get; init; }
    public string UserId { get; init; }
    public string Token { get; init; }
    public TokenType TokenType { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedBy { get; private set; }

    private AuthToken(string userId, string token, TokenType tokenType, DateTime expiresAt)
    {
        Id = Guid.NewGuid().ToString();
        UserId = userId;
        Token = token;
        TokenType = tokenType;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public static AuthToken Create(string userId, string token, TokenType tokenType, DateTime expiresAt)
    {
        var authToken = new AuthToken(userId, token, tokenType, expiresAt);

        var validator = new AuthTokenValidator();
        var validationResult = validator.Validate(authToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"AuthToken validation failed: {errors}");
        }

        return authToken;
    }

    public void Revoke(string revokedBy)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsRevoked && !IsExpired;
}

public class AuthTokenValidator : AbstractValidator<AuthToken>
{
    public AuthTokenValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required")
            .MinimumLength(10).WithMessage("Token must be at least 10 characters");

        RuleFor(x => x.TokenType)
            .IsInEnum().WithMessage("Invalid token type");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Token expiration must be in the future");
    }

}