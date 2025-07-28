using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace ProductManagementSystem.Application.Domain.Subscriptions.DTOs.Inputs;

public record SubscriptionFilterDTO
{
    [FromQuery(Name = "name")]
    public string? Name { get; set; }

    [FromQuery(Name = "period")]
    public string? Period { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "page_size")]
    public int PageSize { get; set; } = 10;
}