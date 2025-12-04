using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public record CreateOfferDTO
{

    [Url(ErrorMessage = "Offer URL must be a valid URL")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [Required(ErrorMessage = "Offer price is required")]
    [JsonPropertyName("price")]
    public required MoneyDTO Price { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [JsonPropertyName("min_quantity")]
    public int MinQuantity { get; set; }
}