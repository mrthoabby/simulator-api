using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Outputs;

public struct ConceptCodeDTO
{
    [JsonPropertyName("code")]
    public string Code { get; init; }
}