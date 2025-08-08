using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Inputs;

public record UpdateGlobalParameterDTO
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("application")]
    [EnumDataType(typeof(EnumConceptApplication))]
    public EnumConceptApplication? Application { get; set; }
    [JsonPropertyName("type")]
    [EnumDataType(typeof(EnumConceptType))]
    public EnumConceptType? Type { get; set; }
    [JsonPropertyName("price")]
    public MoneyDTO? Price { get; set; }
    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }
}