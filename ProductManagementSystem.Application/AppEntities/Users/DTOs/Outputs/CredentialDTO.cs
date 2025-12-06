namespace ProductManagementSystem.Application.AppEntities.Users.DTOs.Outputs;

public class CredentialDTO
{
    public string Email { get; set; } = string.Empty;
    // No incluimos Password por seguridad
}