using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using ProductManagementSystem.Application.AppEntities.Subscriptions.Enums;
using FluentValidation;
using System.Linq;

namespace ProductManagementSystem.Application.AppEntities.Subscriptions.Models;

public class Subscription
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public Price Price { get; init; }
    public EnumSubscriptionPeriod Period { get; init; }
    public Restrictions Restrictions { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    private Subscription(string name, string description, Price price, EnumSubscriptionPeriod period, Restrictions restrictions)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Description = description;
        Price = price;
        Period = period;
        Restrictions = restrictions;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Validar la entidad usando FluentValidation
        var validator = new SubscriptionValidator();
        var validationResult = validator.Validate(this);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Subscription validation failed: {errors}");
        }
    }

    public static Subscription Create(string name, string description, Price price, EnumSubscriptionPeriod period, Restrictions restrictions)
    {
        return new Subscription(name, description, price, period, restrictions);
    }


}

public class SubscriptionValidator : AbstractValidator<Subscription>
{
    public SubscriptionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subscription name is required")
            .MaximumLength(100).WithMessage("Subscription name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Subscription description is required")
            .MaximumLength(500).WithMessage("Subscription description cannot exceed 500 characters");

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Subscription price is required");

        RuleFor(x => x.Period)
            .IsInEnum().WithMessage("Invalid subscription period");

        RuleFor(x => x.Restrictions)
            .NotNull().WithMessage("Subscription restrictions are required");
    }
}