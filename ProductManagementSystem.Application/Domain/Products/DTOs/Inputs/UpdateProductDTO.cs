using System.ComponentModel.DataAnnotations;
using ProductManagementSystem.Application.Domain.Shared.DTOs;

namespace ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;

public class UpdateProductDTO
{
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string? Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public MoneyDTO? Price { get; set; }

    [Url(ErrorMessage = "Image URL must be a valid URL")]
    public string? ImageUrl { get; set; }

}