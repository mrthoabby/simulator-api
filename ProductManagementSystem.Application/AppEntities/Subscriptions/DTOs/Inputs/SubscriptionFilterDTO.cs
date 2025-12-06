using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.Application.AppEntities.Subscriptions.DTOs.Inputs;

public record SubscriptionFilterDTO
{
    [FromQuery(Name = "name")]
    [StringLength(100, ErrorMessage = "Name filter cannot exceed 100 characters")]
    public string? Name { get; init; }

    [FromQuery(Name = "period")]
    [StringLength(50, ErrorMessage = "Period filter cannot exceed 50 characters")]
    public string? Period { get; init; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; init; }

    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; init; }

    [FromQuery(Name = "page_size")]
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; init; }
}