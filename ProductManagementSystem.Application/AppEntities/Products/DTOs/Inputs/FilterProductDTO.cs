using Microsoft.AspNetCore.Mvc;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public class FilterProductDTO
{
    [FromQuery(Name = "createdAt")]
    public DateTime? CreatedAt { get; set; }

    [FromQuery(Name = "updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}