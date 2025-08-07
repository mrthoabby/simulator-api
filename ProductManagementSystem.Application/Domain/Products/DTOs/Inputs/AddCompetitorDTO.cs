using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Domain.Shared.DTOs;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;

public struct AddCompetitorDTO
{
    [Required(ErrorMessage = "Competitor URL is required")]
    [Url(ErrorMessage = "Competitor URL must be a valid URL")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [Url(ErrorMessage = "Image URL must be a valid URL")]
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Competitor price is required")]
    [JsonPropertyName("price")]
    public MoneyDTO Price { get; set; }

}