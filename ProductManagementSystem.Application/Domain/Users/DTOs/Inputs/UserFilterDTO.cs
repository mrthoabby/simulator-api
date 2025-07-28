using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Domain.Users.Models;

namespace ProductManagementSystem.Application.Domain.Users.DTOs.Inputs;

public record UserFilterDTO : IUserFilters
{
    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int? Page { get; set; }

    [FromQuery(Name = "page_size")]
    [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")]
    public int? PageSize { get; set; }

    [FromQuery(Name = "name")]
    public string? Name { get; set; }

    [FromQuery(Name = "email")]
    public string? Email { get; set; }

    [FromQuery(Name = "company_id")]
    public string? CompanyId { get; set; }

    [FromQuery(Name = "subscription_id")]
    public string? SubscriptionId { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "with_user_plans")]
    public bool WithUserPlans { get; set; }
}