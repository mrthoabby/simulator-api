using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;

public record CreateDeductionCodeDTO
{
    [Required(ErrorMessage = "Deduction code is required")]
    [StringLength(50, ErrorMessage = "Deduction code cannot exceed 50 characters")]
    [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Deduction code must contain only uppercase letters and underscores")]
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("is_from_system")]
    public bool? IsFromSystem { get; init; }
}