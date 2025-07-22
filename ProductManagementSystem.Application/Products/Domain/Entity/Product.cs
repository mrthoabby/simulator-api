using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Products.Domain.Type;

namespace ProductManagementSystem.Application.Products.Models.Entity;

public class Product
{
    public required string Name { get; set; }
    public required Price Price { get; set; }
    public string? ImageUrl { get; set; }

    // Nested objects as specified
    public required List<Deduction> Deductions { get; set; }
    public required List<Provider> Providers { get; set; }
    public required List<Competitor> Competitors { get; set; }
}