using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public class UpdateProductImageDTO
{
    [Required(ErrorMessage = "Image URL is required")]
    [Url(ErrorMessage = "Image URL must be a valid URL")]
    public string ImageUrl { get; set; } = string.Empty;
}