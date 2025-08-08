using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.Enum;

namespace ProductManagementSystem.Application.Domain.Shared.DTOs;

public record MoneyDTO
{
    [JsonPropertyName("value")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
    public required decimal Value { get; set; }

    [JsonPropertyName("currency")]
    [EnumDataType(typeof(EnumCurrency), ErrorMessage = "Invalid currency")]
    public required EnumCurrency Currency { get; set; }
}