using FluentValidation;
using ProductManagementSystem.Application.Common.AppEntities.Type;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Models;

public class Quotation : Entity
{
    public string ProductId { get; private set; }
    public string ProviderId { get; private set; }
    public string ProviderName { get; private set; }
    public Dimensions Dimensions { get; private set; }
    public int UnitsPerBox { get; private set; }
    public int TotalUnits { get; private set; }
    public bool IsActive { get; private set; }

    private Quotation(string productId, string providerId, string providerName, Dimensions dimensions, int unitsPerBox, int totalUnits, bool isActive) : base()
    {
        ProductId = productId;
        ProviderId = providerId;
        ProviderName = providerName;
        Dimensions = dimensions;
        UnitsPerBox = unitsPerBox;
        TotalUnits = totalUnits;
        IsActive = isActive;
    }

    public static Quotation Create(string productId, string providerId, string providerName, Dimensions dimensions, int unitsPerBox, int totalUnits, bool isActive = true)
    {
        var quotation = new Quotation(productId, providerId, providerName, dimensions, unitsPerBox, totalUnits, isActive);
        var validator = new QuotationValidator();
        var validationResult = validator.Validate(quotation);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        return quotation;
    }

    public void Update(Dimensions? dimensions, int? unitsPerBox, int? totalUnits, bool? isActive)
    {
        if (dimensions != null)
            Dimensions = dimensions;
        if (unitsPerBox.HasValue)
            UnitsPerBox = unitsPerBox.Value;
        if (totalUnits.HasValue)
            TotalUnits = totalUnits.Value;
        if (isActive.HasValue)
            IsActive = isActive.Value;
        
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }
}

public class QuotationValidator : AbstractValidator<Quotation>
{
    public QuotationValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required");

        RuleFor(x => x.ProviderName)
            .NotEmpty().WithMessage("Provider name is required");

        RuleFor(x => x.Dimensions)
            .NotNull().WithMessage("Dimensions are required");

        RuleFor(x => x.UnitsPerBox)
            .GreaterThanOrEqualTo(0).WithMessage("Units per box must be greater than or equal to 0");

        RuleFor(x => x.TotalUnits)
            .GreaterThanOrEqualTo(0).WithMessage("Total units must be greater than or equal to 0");
    }
}

