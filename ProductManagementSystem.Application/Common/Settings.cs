namespace ProductManagementSystem.Application.Common;

public class SecuritySettings
{
    public string PasswordPepper { get; set; } = string.Empty;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Comma or semicolon separated list of valid audiences.
    /// Example: "https://web.domain.com,extension-key-123,https://mobile.domain.com"
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    public int ExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInDays { get; set; } = 7;

    /// <summary>
    /// Returns the list of valid audiences parsed from the Audience string.
    /// Supports comma (,) and semicolon (;) as separators.
    /// </summary>
    public IEnumerable<string> GetAudiences()
    {
        if (string.IsNullOrWhiteSpace(Audience))
            return Enumerable.Empty<string>();

        return Audience
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrEmpty(a));
    }

    /// <summary>
    /// Returns the first audience (used when creating tokens).
    /// </summary>
    public string GetPrimaryAudience()
    {
        return GetAudiences().FirstOrDefault() ?? string.Empty;
    }
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