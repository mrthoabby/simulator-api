using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Common.Domain.Enum;
using ProductManagementSystem.Application.Domain.Subscriptions.Enums;

namespace ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Requests;

public record CreateSubscriptionDTO
{
    [Required(ErrorMessage = "Subscription name is required")]
    [StringLength(100, ErrorMessage = "Subscription name cannot exceed 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subscription description is required")]
    [StringLength(500, ErrorMessage = "Subscription description cannot exceed 500 characters")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [EnumDataType(typeof(EnumCurrency), ErrorMessage = "Invalid currency")]
    [JsonPropertyName("currency")]
    public EnumCurrency Currency { get; set; }

    [Required(ErrorMessage = "Subscription period is required")]
    [EnumDataType(typeof(EnumSubscriptionPeriod), ErrorMessage = "Invalid subscription period")]
    [JsonPropertyName("period")]
    public EnumSubscriptionPeriod Period { get; set; }

    // Restrictions
    [Range(0, int.MaxValue, ErrorMessage = "Max products must be greater than or equal to 0")]
    [JsonPropertyName("max_products")]
    public int MaxProducts { get; set; } = 0;

    [Range(1, int.MaxValue, ErrorMessage = "Max users must be greater than 0")]
    [JsonPropertyName("max_users")]
    public int MaxUsers { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Max competitors must be greater than or equal to 0")]
    [JsonPropertyName("max_competitors")]
    public int MaxCompetitors { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Max custom deductions must be greater than or equal to 0")]
    [JsonPropertyName("max_custom_deductions")]
    public int MaxCustomDeductions { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Max simulations must be greater than or equal to 0")]
    [JsonPropertyName("max_simulations")]
    public int MaxSimulations { get; set; } = 0;

    [JsonPropertyName("is_pdf_export_supported")]
    public bool IsPDFExportSupported { get; set; } = false;

    [JsonPropertyName("is_simulation_comparison_supported")]
    public bool IsSimulationComparisonSupported { get; set; } = false;

    [JsonPropertyName("is_excel_export_supported")]
    public bool IsExcelExportSupported { get; set; } = false;

}