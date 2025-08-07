using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Type;

namespace ProductManagementSystem.Application.Domain.Products.Models;

public class Offer
{
    public string Url { get; set; } = string.Empty;
    public Money Price { get; set; }
    public int MinQuantity { get; set; }

    private Offer(string url, Money price, int minQuantity)
    {
        Url = url;
        Price = price;
        MinQuantity = minQuantity;
    }

    public static Offer Create(string url, Money price, int minQuantity)
    {
        var offer = new Offer(url, price, minQuantity);
        var validator = new OfferValidator();
        var validationResult = validator.Validate(offer);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        return offer;
    }
}

// Validador de FluentValidation para Offer
public class OfferValidator : AbstractValidator<Offer>
{
    public OfferValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Offer URL is required")
            .Must(BeAValidUrl).WithMessage("Offer URL must be a valid URL");

        RuleFor(x => x.MinQuantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Offer price is required");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}