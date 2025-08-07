using FluentValidation;

namespace ProductManagementSystem.Application.Domain.Products.Models;

public class Provider
{
    public string Name { get; set; }
    public string Url { get; set; }
    public List<Offer>? Offers { get; set; }


    private Provider(string name, string url, List<Offer> offers)
    {
        Name = name;
        Url = url;
        Offers = offers;
    }

    public static Provider Create(string name, string url, List<Offer> offers)
    {
        var provider = new Provider(name, url, offers);
        var validator = new ProviderValidator();
        var validationResult = validator.Validate(provider);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        return provider;
    }

    public void AddOffer(Offer offer)
    {
        if (Offers == null)
        {
            Offers = new List<Offer>();
        }
        Offers.Add(offer);
    }
}

// Validador de FluentValidation para Provider
public class ProviderValidator : AbstractValidator<Provider>
{
    public ProviderValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Provider name is required")
            .MaximumLength(100).WithMessage("Provider name cannot exceed 100 characters");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Provider URL is required")
            .Must(BeAValidUrl).WithMessage("Provider URL must be a valid URL");


    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}