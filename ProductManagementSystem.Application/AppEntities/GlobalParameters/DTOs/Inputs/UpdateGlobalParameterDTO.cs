using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;

namespace ProductManagementSystem.Application.AppEntities.GlobalParameters.DTOs.Inputs;

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