using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public class AddProviderDTO
{
    [Required(ErrorMessage = "Provider name is required")]
    [StringLength(100, ErrorMessage = "Provider name cannot exceed 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Url(ErrorMessage = "Provider URL must be a valid URL")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("offers")]
    public List<CreateOfferDTO>? Offers { get; set; }
}