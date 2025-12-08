using FluentValidation;

namespace ProductManagementSystem.Application.AppEntities.Quotations.Models;

public class Dimensions
{
    public decimal Width { get; private set; }
    public decimal Height { get; private set; }
    public decimal Depth { get; private set; }

    private Dimensions(decimal width, decimal height, decimal depth)
    {
        Width = width;
        Height = height;
        Depth = depth;
    }

    public static Dimensions Create(decimal width, decimal height, decimal depth)
    {
        var dimensions = new Dimensions(width, height, depth);
        var validator = new DimensionsValidator();
        var validationResult = validator.Validate(dimensions);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        return dimensions;
    }

    public decimal GetVolume() => Width * Height * Depth;
}

public class DimensionsValidator : AbstractValidator<Dimensions>
{
    public DimensionsValidator()
    {
        RuleFor(x => x.Width)
            .GreaterThanOrEqualTo(0).WithMessage("Width must be greater than or equal to 0");

        RuleFor(x => x.Height)
            .GreaterThanOrEqualTo(0).WithMessage("Height must be greater than or equal to 0");

        RuleFor(x => x.Depth)
            .GreaterThanOrEqualTo(0).WithMessage("Depth must be greater than or equal to 0");
    }
}

