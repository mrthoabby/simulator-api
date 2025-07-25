using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Outputs;

public record SubscriptionDTO
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public required string Description { get; set; }
    [JsonPropertyName("price")]
    public required PriceDTO Price { get; set; }
    [JsonPropertyName("period")]
    public required string Period { get; set; }
    [JsonPropertyName("restrictions")]
    public required RestrictionsDTO Restrictions { get; set; }
    [JsonPropertyName("is_active")]
    public required bool IsActive { get; set; }
    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public required DateTime UpdatedAt { get; set; }
}

public record PriceDTO
{
    [JsonPropertyName("value")]
    public required decimal Value { get; set; }
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
}

public record RestrictionsDTO
{
    [JsonPropertyName("max_products")]
    public required int MaxProducts { get; set; }
    [JsonPropertyName("max_users")]
    public required int MaxUsers { get; set; }
    [JsonPropertyName("max_competitors")]
    public required int MaxCompetitors { get; set; }
    [JsonPropertyName("max_custom_deductions")]
    public required int MaxCustomDeductions { get; set; }
    [JsonPropertyName("max_simulations")]
    public required int MaxSimulations { get; set; }
    [JsonPropertyName("is_pdf_export_supported")]
    public required bool IsPDFExportSupported { get; set; }
    [JsonPropertyName("is_simulation_comparison_supported")]
    public required bool IsSimulationComparisonSupported { get; set; }
    [JsonPropertyName("is_excel_export_supported")]
    public required bool IsExcelExportSupported { get; set; }
}