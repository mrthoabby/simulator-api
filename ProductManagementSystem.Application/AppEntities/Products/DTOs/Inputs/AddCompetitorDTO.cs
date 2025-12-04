using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public record AddCompetitorDTO
{
    [Required(ErrorMessage = "Competitor name is required")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [Url(ErrorMessage = "Competitor URL must be a valid URL")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [Url(ErrorMessage = "Image URL must be a valid URL")]
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Competitor price is required")]
    [JsonPropertyName("price")]
    public required MoneyDTO Price { get; set; }

}