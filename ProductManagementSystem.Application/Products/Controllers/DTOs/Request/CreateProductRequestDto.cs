using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProductManagementSystem.Application.Common.Domain.Enum;
using ProductManagementSystem.Application.Products.Domain.Type;
using ProductManagementSystem.Application.Products.Commands.CreateProduct;
using static ProductManagementSystem.Application.Products.Commands.CreateProduct.CreateProductCommand;
using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Products.Controllers.DTOs.Request;

public class CreateProductRequestDto
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Product name is required")]
    [MinLength(1, ErrorMessage = "Product name cannot be empty")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price_value")]
    [Required(ErrorMessage = "Price value is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price value must be greater than 0")]
    public decimal PriceValue { get; set; }

    [JsonPropertyName("price_currency")]
    [Required(ErrorMessage = "Price currency is required")]
    [EnumDataType(typeof(EnumCurrency), ErrorMessage = "Currency must be a valid currency type")]
    public string PriceCurrency { get; set; } = string.Empty;

    [JsonPropertyName("image_url")]
    [Url(ErrorMessage = "Image URL must be a valid URL")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("deductions")]
    public List<DeductionDto>? Deductions { get; set; }

    [JsonPropertyName("providers")]
    public List<ProviderDto>? Providers { get; set; }

    [JsonPropertyName("competitors")]
    public List<CompetitorDto>? Competitors { get; set; }

    public CreateProductCommand ToCommand()
    {
        var builder = new CreateCommandBuilder(new CreateProductValidator())
            .WithName(Name)
            .WithPrice(new Price(PriceValue, PriceCurrency.ToEnumCurrency()));


        if (ImageUrl != null)
            builder.WithImageUrl(ImageUrl);

        if (Deductions != null)
            builder.WithDeductions(Deductions
            .Select(d =>
            new Deduction(
            d.Name,
            d.Description,
            d.ConceptCode,
            new DeductionValue(
                d.Value,
                d.Type.ToEnumDeductionType()),
                d.Application.ToEnumDeductionApplication())).ToList());

        if (Providers != null)
            builder.WithProviders(Providers
            .Select(p =>
            new Provider(
                p.Name,
                p.Url,
                p.Offers
                .Select(o => new Offer(
                    o.Url, new Price(
                        o.PriceValue,
                        o.PriceCurrency.ToEnumCurrency()),
                        o.Quantity)).ToList())).ToList());

        if (Competitors != null)
            builder.WithCompetitors(Competitors
            .Select(c => new Competitor(
                c.Url,
                c.ImageUrl, new Price(
                    c.PriceValue,
                    c.PriceCurrency.ToEnumCurrency()))).ToList());

        return builder.Build();
    }
}
