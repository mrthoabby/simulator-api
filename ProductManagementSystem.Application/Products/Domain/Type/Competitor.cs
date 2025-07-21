using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Products.Models.Type;

public record Competitor(string productUrl, string imageUrl, Price price);
