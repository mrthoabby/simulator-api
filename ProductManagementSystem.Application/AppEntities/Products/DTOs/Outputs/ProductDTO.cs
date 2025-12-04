using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;

public record ProductDTO
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("price")]
    public required MoneyDTO Price { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("concepts")]
    public List<ConceptDTO>? Concepts { get; set; }

    [JsonPropertyName("providers")]
    public List<ProviderDTO>? Providers { get; set; }

    [JsonPropertyName("competitors")]
    public List<CompetitorDTO>? Competitors { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required DateTime UpdatedAt { get; set; }
}