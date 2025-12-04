using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public class AddConceptDTO
{
    [Required(ErrorMessage = "Concept code is required")]
    [StringLength(50, ErrorMessage = "Concept code cannot exceed 50 characters")]
    [JsonPropertyName("concept_code")]
    public string ConceptCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Type is required")]
    [EnumDataType(typeof(EnumConceptType), ErrorMessage = "Invalid type")]
    [JsonPropertyName("type")]
    public required EnumConceptType Type { get; set; }

    [Required(ErrorMessage = "Application is required")]
    [EnumDataType(typeof(EnumConceptApplication), ErrorMessage = "Invalid application")]
    [JsonPropertyName("application")]
    public required EnumConceptApplication Application { get; set; }

    [Range(0.01, 100, ErrorMessage = "Percentage must be between 0.01 and 100")]
    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }

    [JsonPropertyName("price")]
    public MoneyDTO? Price { get; set; }
}