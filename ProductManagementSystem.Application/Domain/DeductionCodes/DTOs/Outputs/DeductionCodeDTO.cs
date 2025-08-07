using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Outputs;

public struct DeductionCodeDTO
{
    [JsonPropertyName("code")]
    public string Code { get; init; }
}