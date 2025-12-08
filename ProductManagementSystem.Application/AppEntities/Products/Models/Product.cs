using FluentValidation;
using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.Products.Models;

public class Product
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string? ImageUrl { get; private set; }
    public List<Concept>? Concepts { get; private set; }
    public List<Provider>? Providers { get; private set; }
    public List<Competitor>? Competitors { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Product(string name)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public class ProductBuilder
    {
        private string _name;
        private string? _imageUrl;
        private List<Concept>? _concepts;
        private List<Provider>? _providers;
        private List<Competitor>? _competitors;

        public ProductBuilder(string name)
        {
            _name = name;
        }

        public ProductBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ProductBuilder WithImageUrl(string? imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public ProductBuilder WithConcepts(List<Concept> concepts)
        {
            _concepts = concepts;
            return this;
        }

        public ProductBuilder WithProviders(List<Provider> providers)
        {
            _providers = providers;
            return this;
        }

        public ProductBuilder WithCompetitors(List<Competitor> competitors)
        {
            _competitors = competitors;
            return this;
        }

        public Product Build()
        {
            var product = new Product(_name);
            var validator = new ProductValidator();
            var validationResult = validator.Validate(new Product(_name));
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                throw new ValidationException(string.Join(", ", errors));
            }

            product.ImageUrl = _imageUrl;
            product.Concepts = _concepts;
            product.Providers = _providers;
            product.Competitors = _competitors;

            return product;
        }
    }

    public static ProductBuilder Create(string name)
    {
        return new ProductBuilder(name);
    }
}

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Image URL must be a valid URL");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}