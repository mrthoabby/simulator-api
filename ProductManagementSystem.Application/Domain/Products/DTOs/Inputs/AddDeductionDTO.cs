using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;

public class AddDeductionDTO
{
    [Required(ErrorMessage = "Deduction concept code is required")]
    [StringLength(50, ErrorMessage = "Deduction concept code cannot exceed 50 characters")]
    [JsonPropertyName("concept_code")]
    public string ConceptCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Deduction name is required")]
    [StringLength(200, ErrorMessage = "Deduction name cannot exceed 200 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Deduction description cannot exceed 500 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Deduction type is required")]
    [EnumDataType(typeof(EnumDeductionType), ErrorMessage = "Invalid deduction type")]
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [Required(ErrorMessage = "Deduction application is required")]
    [EnumDataType(typeof(EnumDeductionApplication), ErrorMessage = "Invalid deduction application")]
    [JsonPropertyName("application")]
    public required string Application { get; set; }

    [Range(0.01, 100, ErrorMessage = "Deduction percentage must be between 0.01 and 100")]
    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }

    [JsonPropertyName("price")]
    public MoneyDTO? Price { get; set; }
}