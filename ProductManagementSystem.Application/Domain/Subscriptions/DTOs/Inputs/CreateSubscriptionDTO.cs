using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Common.Domain.Enum;
using ProductManagementSystem.Application.Domain.Subscriptions.Enums;

namespace ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;

public struct CreateSubscriptionDTO
{
    [Required(ErrorMessage = "Subscription name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Subscription name must be between 2 and 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [Required(ErrorMessage = "Subscription description is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Subscription description must be between 10 and 500 characters")]
    [JsonPropertyName("description")]
    public string Description { get; init; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [Required(ErrorMessage = "Currency is required")]
    [EnumDataType(typeof(EnumCurrency), ErrorMessage = "Invalid currency")]
    [JsonPropertyName("currency")]
    public EnumCurrency Currency { get; init; }

    [Required(ErrorMessage = "Subscription period is required")]
    [EnumDataType(typeof(EnumSubscriptionPeriod), ErrorMessage = "Invalid subscription period")]
    [JsonPropertyName("period")]
    public EnumSubscriptionPeriod Period { get; init; }

    // Restrictions
    [Range(0, 10000, ErrorMessage = "Max products must be between 0 and 10,000")]
    [JsonPropertyName("max_products")]
    public int MaxProducts { get; init; }

    [Range(1, 1000, ErrorMessage = "Max users must be between 1 and 1,000")]
    [JsonPropertyName("max_users")]
    public int MaxUsers { get; init; }

    [Range(0, 1000, ErrorMessage = "Max competitors must be between 0 and 1,000")]
    [JsonPropertyName("max_competitors")]
    public int MaxCompetitors { get; init; }

    [Range(0, 1000, ErrorMessage = "Max custom deductions must be between 0 and 1,000")]
    [JsonPropertyName("max_custom_deductions")]
    public int MaxCustomDeductions { get; init; }

    [Range(0, 10000, ErrorMessage = "Max simulations must be between 0 and 10,000")]
    [JsonPropertyName("max_simulations")]
    public int MaxSimulations { get; init; }

    [JsonPropertyName("is_pdf_export_supported")]
    public bool IsPDFExportSupported { get; init; }

    [JsonPropertyName("is_simulation_comparison_supported")]
    public bool IsSimulationComparisonSupported { get; init; }

    [JsonPropertyName("is_excel_export_supported")]
    public bool IsExcelExportSupported { get; init; }
}