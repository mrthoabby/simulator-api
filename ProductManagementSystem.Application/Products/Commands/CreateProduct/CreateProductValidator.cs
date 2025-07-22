using FluentValidation;
using ProductManagementSystem.Application.Products.Domain.Type;

namespace ProductManagementSystem.Application.Products.Commands.CreateProduct;
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(CreateProductValidationMessages.PRODUCT_NAME_REQUIRED)
            .MaximumLength(200).WithMessage(CreateProductValidationMessages.PRODUCT_NAME_MAX_LENGTH);

        RuleFor(x => x.Price)
            .NotNull().WithMessage(CreateProductValidationMessages.PRICE_REQUIRED);

        RuleFor(x => x.Price.Value)
            .GreaterThan(0).WithMessage(CreateProductValidationMessages.PRICE_VALUE_GREATER_THAN_0);

        RuleFor(x => x.Price.Currency)
            .IsInEnum().WithMessage(CreateProductValidationMessages.INVALID_CURRENCY);

        RuleFor(x => x.ImageUrl)
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage(CreateProductValidationMessages.IMAGE_URL_VALID);

        RuleForEach(x => x.Deductions)
            .SetValidator(new DeductionValidator());

        RuleForEach(x => x.Providers)
            .SetValidator(new ProviderValidator());

        RuleForEach(x => x.Competitors)
            .SetValidator(new CompetitorValidator());
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class DeductionValidator : AbstractValidator<Deduction>
{
    public DeductionValidator()
    {
        RuleFor(x => x.ConceptCode)
            .NotEmpty().WithMessage(CreateProductValidationMessages.DEDUCTION_CONCEPT_CODE_REQUIRED)
            .MaximumLength(50).WithMessage(CreateProductValidationMessages.DEDUCTION_CONCEPT_CODE_MAX_LENGTH);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(CreateProductValidationMessages.DEDUCTION_NAME_REQUIRED)
            .MaximumLength(200).WithMessage(CreateProductValidationMessages.DEDUCTION_NAME_MAX_LENGTH);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(CreateProductValidationMessages.DEDUCTION_DESCRIPTION_MAX_LENGTH);

        RuleFor(x => x.Value.Value)
            .GreaterThan(0).WithMessage(CreateProductValidationMessages.DEDUCTION_VALUE_GREATER_THAN_0);

        RuleFor(x => x.Value.Type)
            .IsInEnum().WithMessage(CreateProductValidationMessages.INVALID_DEDUCTION_TYPE);

        RuleFor(x => x.Application)
            .IsInEnum().WithMessage(CreateProductValidationMessages.INVALID_DEDUCTION_APPLICATION);
    }
}

public class ProviderValidator : AbstractValidator<Provider>
{
    public ProviderValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(CreateProductValidationMessages.PROVIDER_NAME_REQUIRED)
            .MaximumLength(100).WithMessage(CreateProductValidationMessages.PROVIDER_NAME_MAX_LENGTH);

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage(CreateProductValidationMessages.PROVIDER_URL_REQUIRED)
            .Must(BeValidUrl).WithMessage(CreateProductValidationMessages.PROVIDER_URL_VALID);

        RuleForEach(x => x.Offers)
            .SetValidator(new OfferValidator());
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class OfferValidator : AbstractValidator<Offer>
{
    public OfferValidator()
    {
        RuleFor(x => x.Price.Value)
            .GreaterThan(0).WithMessage(CreateProductValidationMessages.OFFER_PRICE_GREATER_THAN_0);

        RuleFor(x => x.MinimumQuantity)
            .GreaterThan(0).WithMessage(CreateProductValidationMessages.MINIMUM_QUANTITY_GREATER_THAN_0);
    }
}

public class CompetitorValidator : AbstractValidator<Competitor>
{
    public CompetitorValidator()
    {
        RuleFor(x => x.productUrl)
            .NotEmpty().WithMessage(CreateProductValidationMessages.PRODUCT_URL_REQUIRED)
            .Must(BeValidUrl).WithMessage(CreateProductValidationMessages.PRODUCT_URL_VALID);

        RuleFor(x => x.imageUrl)
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.imageUrl))
            .WithMessage(CreateProductValidationMessages.IMAGE_URL_VALID);

        RuleFor(x => x.price.Value)
            .GreaterThan(0).WithMessage(CreateProductValidationMessages.COMPETITOR_PRICE_GREATER_THAN_0);
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public static class CreateProductValidationMessages
{
    public const string PRODUCT_NAME_REQUIRED = "Product name is required";
    public const string PRODUCT_NAME_MAX_LENGTH = "Product name cannot exceed 200 characters";
    public const string PRICE_REQUIRED = "Price is required";
    public const string PRICE_VALUE_GREATER_THAN_0 = "Price value must be greater than 0";
    public const string INVALID_CURRENCY = "Invalid currency";
    public const string IMAGE_URL_REQUIRED = "Image URL is required";
    public const string IMAGE_URL_VALID = "Image URL must be a valid URL";
    public const string DEDUCTION_CONCEPT_CODE_REQUIRED = "Deduction concept code is required";
    public const string DEDUCTION_CONCEPT_CODE_MAX_LENGTH = "Deduction concept code cannot exceed 50 characters";
    public const string DEDUCTION_NAME_REQUIRED = "Deduction name is required";
    public const string DEDUCTION_NAME_MAX_LENGTH = "Deduction name cannot exceed 200 characters";
    public const string DEDUCTION_DESCRIPTION_MAX_LENGTH = "Deduction description cannot exceed 500 characters";
    public const string DEDUCTION_VALUE_GREATER_THAN_0 = "Deduction value must be greater than 0";
    public const string INVALID_DEDUCTION_TYPE = "Invalid deduction type";
    public const string INVALID_DEDUCTION_APPLICATION = "Invalid deduction application";
    public const string PROVIDER_NAME_REQUIRED = "Provider name is required";
    public const string PROVIDER_NAME_MAX_LENGTH = "Provider name cannot exceed 100 characters";
    public const string PROVIDER_URL_REQUIRED = "Provider URL is required";
    public const string PROVIDER_URL_VALID = "Provider URL must be a valid URL";
    public const string OFFER_PRICE_GREATER_THAN_0 = "Offer price must be greater than 0";
    public const string MINIMUM_QUANTITY_GREATER_THAN_0 = "Minimum quantity must be greater than 0";
    public const string PRODUCT_URL_REQUIRED = "Product URL is required";
    public const string PRODUCT_URL_VALID = "Product URL must be a valid URL";
    public const string COMPETITOR_PRICE_GREATER_THAN_0 = "Competitor price must be greater than 0";
}