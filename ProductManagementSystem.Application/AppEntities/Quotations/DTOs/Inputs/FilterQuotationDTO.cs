using Microsoft.AspNetCore.Mvc;

namespace ProductManagementSystem.Application.AppEntities.Quotations.DTOs.Inputs;

public class FilterQuotationDTO
{
    [FromQuery(Name = "providerId")]
    public string? ProviderId { get; set; }

    [FromQuery(Name = "isActive")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "createdAt")]
    public DateTime? CreatedAt { get; set; }
}

