using FluentValidation;
using System.Security.Cryptography;
using System.Text;

namespace ProductManagementSystem.Application.Domain.Users.Models;

public class Credential
{
    // Propiedades que se guardan en persistencia
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;

    /// <summary>
    /// Crea credential desde datos de persistencia (BD) para validación
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="passwordHash">Hash del password desde BD</param>
    /// <returns>Credential para validación</returns>
    public static Credential FromPersistence(string email, string passwordHash)
    {
        if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrEmpty(passwordHash)) throw new ArgumentException("Password hash is required", nameof(passwordHash));

        return new Credential { Email = email, PasswordHash = passwordHash };
    }

    /// <summary>
    /// Crea una nueva credential hasheando el password con salt aleatorio único y pepper
    /// </summary>
    public static Credential Create(string email, string password, string pepper)
    {
        if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is required", nameof(password));
        if (string.IsNullOrEmpty(pepper)) throw new ArgumentException("Pepper is required", nameof(pepper));

        var passwordHash = GenerateSecurePasswordHash(password, pepper);
        return new Credential { Email = email, PasswordHash = passwordHash };
    }

    /// <summary>
    /// Valida si el password proporcionado coincide con el hash almacenado usando salt original
    /// </summary>
    /// <param name="password">Password en texto plano a validar</param>
    /// <param name="pepper">Pepper usado durante la creación</param>
    /// <returns>True si el password es válido</returns>
    public bool VerifyPassword(string password, string pepper)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(PasswordHash))
            return false;

        try
        {
            var parts = PasswordHash.Split(':');
            if (parts.Length != 2) return false;

            var saltBase64 = parts[0];
            var expectedHashBase64 = parts[1];

            var combinedSalt = Encoding.UTF8.GetBytes(saltBase64 + pepper);

            using var keyDerivation = new Rfc2898DeriveBytes(password, combinedSalt, 100000, HashAlgorithmName.SHA256);
            var hashBytes = keyDerivation.GetBytes(32);
            var actualHashBase64 = Convert.ToBase64String(hashBytes);

            return CryptographicCompare(expectedHashBase64, actualHashBase64);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Genera un hash seguro con salt aleatorio único y pepper
    /// Formato almacenado: "salt:hash" donde ambos están en Base64
    /// </summary>
    /// <param name="password">Password en texto plano</param>
    /// <param name="pepper">Pepper externo para seguridad adicional</param>
    /// <returns>Hash con salt en formato "salt:hash"</returns>
    private static string GenerateSecurePasswordHash(string password, string pepper)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        var saltBytes = new byte[16];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        var combinedSalt = Encoding.UTF8.GetBytes(Convert.ToBase64String(saltBytes) + pepper);

        using var keyDerivation = new Rfc2898DeriveBytes(password, combinedSalt, 100000, HashAlgorithmName.SHA256);
        var hashBytes = keyDerivation.GetBytes(32); // 256 bits

        var saltBase64 = Convert.ToBase64String(saltBytes);
        var hashBase64 = Convert.ToBase64String(hashBytes);

        return $"{saltBase64}:{hashBase64}";
    }



    /// <summary>
    /// Comparación criptográficamente segura para evitar timing attacks
    /// </summary>
    private static bool CryptographicCompare(string expected, string actual)
    {
        if (expected.Length != actual.Length)
            return false;

        var result = 0;
        for (int i = 0; i < expected.Length; i++)
        {
            result |= expected[i] ^ actual[i];
        }

        return result == 0;
    }

    /// <summary>
    /// Crea una nueva instancia con password actualizado usando salt aleatorio nuevo
    /// </summary>
    /// <param name="newPassword">Nuevo password</param>
    /// <param name="pepper">Pepper para el hashing</param>
    /// <returns>Nueva instancia de Credential con password actualizado</returns>
    public Credential ChangePassword(string newPassword, string pepper)
    {
        if (string.IsNullOrEmpty(newPassword))
            throw new ArgumentException("Password cannot be empty", nameof(newPassword));

        var newPasswordHash = GenerateSecurePasswordHash(newPassword, pepper);
        return new Credential { Email = Email, PasswordHash = newPasswordHash };
    }
}

// Validador de FluentValidation para Credential
public class CredentialValidator : AbstractValidator<Credential>
{
    public CredentialValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

    }
}
