using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;

public record UpdateQuotationDTO
{
    [JsonPropertyName("dimensions")]
    public DimensionsDTO? Dimensions { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Units per box must be greater than or equal to 0")]
    [JsonPropertyName("units_per_box")]
    public int? UnitsPerBox { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Total units must be greater than or equal to 0")]
    [JsonPropertyName("total_units")]
    public int? TotalUnits { get; init; }

    [JsonPropertyName("is_active")]
    public bool? IsActive { get; init; }
}

