using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;

public record CreateQuotationDTO
{
    [Required(ErrorMessage = "Product ID is required")]
    [JsonPropertyName("product_id")]
    public required string ProductId { get; init; }

    [Required(ErrorMessage = "Provider ID is required")]
    [JsonPropertyName("provider_id")]
    public required string ProviderId { get; init; }

    [Required(ErrorMessage = "Dimensions are required")]
    [JsonPropertyName("dimensions")]
    public required DimensionsDTO Dimensions { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Units per box must be greater than or equal to 0")]
    [JsonPropertyName("units_per_box")]
    public int UnitsPerBox { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Total units must be greater than or equal to 0")]
    [JsonPropertyName("total_units")]
    public int TotalUnits { get; init; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; } = true;
}

public record DimensionsDTO
{
    [Range(0, double.MaxValue, ErrorMessage = "Width must be greater than or equal to 0")]
    [JsonPropertyName("width")]
    public decimal Width { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "Height must be greater than or equal to 0")]
    [JsonPropertyName("height")]
    public decimal Height { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "Depth must be greater than or equal to 0")]
    [JsonPropertyName("depth")]
    public decimal Depth { get; init; }
}

