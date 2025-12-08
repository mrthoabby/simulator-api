using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public record CreateProductDTO
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [Url(ErrorMessage = "Image URL must be a valid URL")]
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("concepts")]
    public List<AddConceptDTO>? Concepts { get; init; }

    [JsonPropertyName("providers")]
    public List<AddProviderDTO>? Providers { get; init; }

    [JsonPropertyName("competitors")]
    public List<AddCompetitorDTO>? Competitors { get; init; }
}
