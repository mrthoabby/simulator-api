using FluentValidation;
using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.Products.Models;

public class Offer
{
    public string Id { get; private set; }
    public string? Url { get; private set; }
    public Money Price { get; private set; }
    public int MinQuantity { get; private set; }

    private Offer(string url, Money price, int minQuantity)
    {
        Id = Guid.NewGuid().ToString();
        Url = url;
        Price = price;
        MinQuantity = minQuantity;
    }

    private Offer(Money price, int minQuantity)
    {
        Id = Guid.NewGuid().ToString();
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

    public static Offer Create(Money price, int minQuantity)
    {
        var offer = new Offer(price, minQuantity);
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
            .Must((offer, url) => url != null && BeAValidUrl(url))
            .WithMessage("Offer URL must be a valid URL")
            .When(x => x.Url != null);

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