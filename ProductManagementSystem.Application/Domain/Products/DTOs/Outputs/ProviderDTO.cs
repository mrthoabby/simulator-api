using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;

public record ProviderDTO
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("offers")]
    public List<OfferDTO>? Offers { get; set; }
    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }
}