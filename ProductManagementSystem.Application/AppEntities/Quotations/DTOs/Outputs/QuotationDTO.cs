using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;

namespace ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Outputs;

public record QuotationDTO
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("product_id")]
    public required string ProductId { get; init; }

    [JsonPropertyName("provider_id")]
    public required string ProviderId { get; init; }

    [JsonPropertyName("provider_name")]
    public required string ProviderName { get; init; }

    [JsonPropertyName("dimensions")]
    public required DimensionsDTO Dimensions { get; init; }

    [JsonPropertyName("units_per_box")]
    public int UnitsPerBox { get; init; }

    [JsonPropertyName("total_units")]
    public int TotalUnits { get; init; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }
}

