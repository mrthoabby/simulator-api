using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Inputs;

public record CreateConceptCodeDTO
{
    [Required(ErrorMessage = "Concept code is required")]
    [StringLength(50, ErrorMessage = "Concept code cannot exceed 50 characters")]
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("is_from_system")]
    public bool? IsFromSystem { get; init; }
}