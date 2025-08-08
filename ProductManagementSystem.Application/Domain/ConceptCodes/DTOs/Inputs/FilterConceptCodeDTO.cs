
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Inputs;

public record FilterConceptCodeDTO
{
    [JsonPropertyName("search")]
    public string? Search { get; init; }

    [JsonPropertyName("is_from_system")]
    public bool? IsFromSystem { get; init; }
}