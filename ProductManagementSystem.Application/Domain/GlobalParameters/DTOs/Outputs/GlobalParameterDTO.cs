using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;


namespace ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Outputs;

public struct GlobalParameterDTO
{
    [JsonPropertyName("concept_code")]
    public string ConceptCode { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("application")]
    public EnumConceptApplication Application { get; init; }

    [JsonPropertyName("type")]
    public EnumConceptType Type { get; init; }

    [JsonPropertyName("price")]
    public MoneyDTO? Price { get; init; }

    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; init; }


}

