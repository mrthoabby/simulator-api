using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;

public struct CreateProductDTO
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [JsonPropertyName("price")]
    public MoneyDTO Price { get; set; }

    [Url(ErrorMessage = "Image URL must be a valid URL")]
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("concepts")]
    public List<AddConceptDTO>? Concepts { get; set; }

    [JsonPropertyName("providers")]
    public List<AddProviderDTO>? Providers { get; set; }

    [JsonPropertyName("competitors")]
    public List<AddCompetitorDTO>? Competitors { get; set; }
}
