using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Inputs;

public record AddGlobalParameterDTO
{
    [Required(ErrorMessage = "Concept code is required")]
    [JsonPropertyName("concept_code")]
    public required string ConceptCode { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Application is required")]
    [EnumDataType(typeof(EnumConceptApplication), ErrorMessage = "Invalid application")]
    [JsonPropertyName("application")]
    public required EnumConceptApplication Application { get; set; }

    [JsonPropertyName("type")]
    [EnumDataType(typeof(EnumConceptType), ErrorMessage = "Invalid type")]
    public required EnumConceptType Type { get; set; }

    [JsonPropertyName("price")]
    public MoneyDTO? Price { get; set; }

    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }
}