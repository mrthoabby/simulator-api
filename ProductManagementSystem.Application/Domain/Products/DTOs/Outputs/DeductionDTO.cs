using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;

public record DeductionDTO
{
    [JsonPropertyName("concept_code")]
    public required string ConceptCode { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("value")]
    public MoneyDTO? Value { get; set; }
    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }
    [JsonPropertyName("type")]
    public required EnumDeductionType Type { get; set; }
    [JsonPropertyName("application")]
    public required EnumDeductionApplication Application { get; set; }
}