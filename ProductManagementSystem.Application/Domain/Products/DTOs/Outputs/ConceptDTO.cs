using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;

public record ConceptDTO
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [Required(ErrorMessage = "Concept code is required")]
    [JsonPropertyName("concept_code")]
    public required string ConceptCode { get; set; }
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("price")]
    public MoneyDTO? Price { get; set; }
    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }
    [JsonPropertyName("type")]
    [Required(ErrorMessage = "Type is required")]
    [EnumDataType(typeof(EnumConceptType), ErrorMessage = "Invalid type")]
    public required EnumConceptType Type { get; set; }
    [JsonPropertyName("application")]
    [Required(ErrorMessage = "Application is required")]
    [EnumDataType(typeof(EnumConceptApplication), ErrorMessage = "Invalid application")]
    public required EnumConceptApplication Application { get; set; }
}