using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;

public record CompetitorDTO
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("price")]
    public required MoneyDTO Price { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }
}