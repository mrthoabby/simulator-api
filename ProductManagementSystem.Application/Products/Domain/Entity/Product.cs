using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Products.Models.Type;

namespace ProductManagementSystem.Application.Products.Models.Entity;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Name { get; set; }
    public required Price Price { get; set; }
    public string? ImageUrl { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

    // Nested objects as specified
    public required List<Deduction> Deductions { get; set; }
    public required List<Provider> Providers { get; set; }
    public required List<Competitor> Competitors { get; set; }
}