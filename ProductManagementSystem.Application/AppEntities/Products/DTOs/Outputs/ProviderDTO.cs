using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Outputs;

public record ProviderDTO
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("offers")]
    public List<OfferDTO>? Offers { get; set; }
    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }
}