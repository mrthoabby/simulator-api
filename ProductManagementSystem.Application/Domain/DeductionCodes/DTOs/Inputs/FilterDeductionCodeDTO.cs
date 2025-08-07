using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;

public record FilterDeductionCodeDTO
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("pattern")]
    public string? Pattern { get; init; }

    [JsonPropertyName("is_from_system")]
    public bool? IsFromSystem { get; init; }
}