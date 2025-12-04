using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;

public record OfferDTO
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("price")]
    public required MoneyDTO Price { get; set; }

    [JsonPropertyName("min_quantity")]
    public required int MinQuantity { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }
}