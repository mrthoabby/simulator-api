using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public class UpdateProductDTO
{
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string? Name { get; set; }

    [Url(ErrorMessage = "Image URL must be a valid URL")]
    public string? ImageUrl { get; set; }
}