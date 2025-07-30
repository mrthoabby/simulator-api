namespace ProductManagementSystem.Application.Common;

public class SecuritySettings
{
    public string PasswordPepper { get; set; } = string.Empty;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public class AuthSettings
{
    public bool EnableAuth { get; set; } = true;
    public bool RequireEmailConfirmation { get; set; } = false;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutTimeInMinutes { get; set; } = 15;
}