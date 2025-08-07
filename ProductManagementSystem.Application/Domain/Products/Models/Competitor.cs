using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Type;

namespace ProductManagementSystem.Application.Domain.Products.Models;

public class Competitor
{
    public string Url { get; init; }
    public string? ImageUrl { get; init; }
    public Money Price { get; init; }

    private Competitor(string url, Money price, string? imageUrl = null)
    {
        Url = url;
        Price = price;
        ImageUrl = imageUrl;
    }

    public static Competitor Create(string url, Money price, string? imageUrl = null)
    {
        var competitor = new Competitor(url, price, imageUrl);
        var validator = new CompetitorValidator();
        var validationResult = validator.Validate(competitor);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        return competitor;
    }
}

public class CompetitorValidator : AbstractValidator<Competitor>
{
    public CompetitorValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Competitor URL is required")
            .Must(BeAValidUrl).WithMessage("Competitor URL must be a valid URL");

        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Competitor price is required");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Competitor image URL must be a valid URL");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}