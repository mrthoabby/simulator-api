using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.Application.Users.Controllers.DTOs.Requests;

public record AddMemberDTO
{
    [Required(ErrorMessage = "Member name is required")]
    [StringLength(100, ErrorMessage = "Member name cannot exceed 100 characters")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public required string Email { get; set; }
    public string? Password { get; set; }
}