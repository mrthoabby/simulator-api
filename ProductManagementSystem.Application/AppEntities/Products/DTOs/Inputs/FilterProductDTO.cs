using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;

namespace ProductManagementSystem.Application.AppEntities.Products.DTOs.Inputs;

public class FilterProductDTO
{
    [FromQuery(Name = "minPrice")]
    public decimal? MinPrice { get; set; }

    [FromQuery(Name = "maxPrice")]
    public decimal? MaxPrice { get; set; }

    [FromQuery(Name = "currency")]
    [EnumDataType(typeof(EnumCurrency))]
    public EnumCurrency? Currency { get; set; }

    [FromQuery(Name = "createdAt")]
    public DateTime? CreatedAt { get; set; }

    [FromQuery(Name = "updatedAt")]
    public DateTime? UpdatedAt { get; set; }

}