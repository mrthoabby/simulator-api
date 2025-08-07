using FluentValidation;
using ProductManagementSystem.Application.Domain.Shared.Type;

namespace ProductManagementSystem.Application.Domain.Products.Models;

public class Competitor
{
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public string? Url { get; private set; }
    public string? ImageUrl { get; private set; }

    private Competitor(string name, Money price, string? url = null, string? imageUrl = null)
    {
        Name = name;
        Price = price;
        Url = url;
        ImageUrl = imageUrl;
    }


    public static Competitor Create(string name, Money price, string? url = null, string? imageUrl = null)
    {
        var competitor = new Competitor(name, price, url, imageUrl);
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
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Competitor name is required");

        RuleFor(x => x.Url)
            .Must((competitor, url) => url != null && BeAValidUrl(url))
            .WithMessage("Competitor URL must be a valid URL")
            .When(x => x.Url != null);

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