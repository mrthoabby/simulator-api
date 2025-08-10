using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Outputs;

public record ConceptCodeDTO
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }
}