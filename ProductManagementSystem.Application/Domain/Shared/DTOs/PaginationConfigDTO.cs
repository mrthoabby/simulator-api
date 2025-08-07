using Microsoft.AspNetCore.Mvc;

namespace ProductManagementSystem.Application.Domain.Shared.DTOs;

public class PaginationConfigDTO
{
    [FromQuery(Name = "page")]
    public int Page { get; set; }
    [FromQuery(Name = "page_size")]
    public int PageSize { get; set; }
}