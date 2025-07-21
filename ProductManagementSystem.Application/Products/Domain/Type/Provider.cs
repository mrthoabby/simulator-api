namespace ProductManagementSystem.Application.Products.Models;

public record Provider(string Name, string Url, List<Offer> Offers);