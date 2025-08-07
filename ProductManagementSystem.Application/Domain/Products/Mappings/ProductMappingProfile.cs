using AutoMapper;
using ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Products.Models;
using ProductManagementSystem.Application.Domain.Shared.Type;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Domain.Shared.Enum;


namespace ProductManagementSystem.Application.Domain.Products.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Base Money mapping - esto permite el mapeo automático en todos los DTOs
        CreateMap<Money, MoneyDTO>();
        CreateMap<MoneyDTO, Money>()
            .ConstructUsing(dto => Money.Create(dto.Value, dto.Currency));

        // Product mappings
        CreateMap<Product, ProductDTO>();
        CreateMap<ProductDTO, Product>()
            .ConstructUsing(dto => Product.Create(dto.Name, Money.Create(dto.Price.Value, dto.Price.Currency)).Build());
        CreateMap<CreateProductDTO, Product>()
            .ConstructUsing(dto => Product.Create(dto.Name, Money.Create(dto.Price.Value, dto.Price.Currency))
                .WithImageUrl(dto.ImageUrl)
                .Build());

        // Provider mappings
        CreateMap<Provider, ProviderDTO>();
        CreateMap<AddProviderDTO, Provider>()
            .ConvertUsing(src => CreateProvider(src));

        // Offer mappings
        CreateMap<Offer, OfferDTO>();
        CreateMap<CreateOfferDTO, Offer>()
            .ConvertUsing(src => CreateOffer(src));

        // Competitor mappings
        CreateMap<Competitor, CompetitorDTO>();
        CreateMap<AddCompetitorDTO, Competitor>()
            .ConstructUsing(src => Competitor.Create(src.Name, Money.Create(src.Price.Value, src.Price.Currency), src.Url, src.ImageUrl));

        // Deduction mappings
        CreateMap<Deduction, DeductionDTO>();
        CreateMap<AddDeductionDTO, Deduction>()
            .ConvertUsing(src => CreateDeduction(src));
    }


    private static Provider CreateProvider(AddProviderDTO src)
    {
        var offers = src.Offers != null && src.Offers.Any()
            ? src.Offers.Select(CreateOffer).ToList()
            : new List<Offer>();

        return Provider.Create(src.Name, src.Url, offers);
    }

    private static Offer CreateOffer(CreateOfferDTO src)
    {
        var money = Money.Create(src.Price.Value, src.Price.Currency);

        return !string.IsNullOrEmpty(src.Url)
            ? Offer.Create(src.Url, money, src.MinQuantity)
            : Offer.Create(money, src.MinQuantity);
    }

    private static Deduction CreateDeduction(AddDeductionDTO src)
    {
        return src.Type switch
        {
            EnumDeductionType.Percentage when src.Percentage.HasValue =>
                Deduction.Create(src.ConceptCode, src.Name, src.Application, src.Percentage.Value, src.Description),

            EnumDeductionType.FixedValue when src.Price != null =>
                Deduction.Create(src.ConceptCode, src.Name, src.Application, Money.Create(src.Price.Value, src.Price.Currency), src.Description),

            EnumDeductionType.Percentage when !src.Percentage.HasValue =>
                Deduction.Create(src.ConceptCode, src.Name, src.Application, src.Description),

            EnumDeductionType.FixedValue when src.Price == null =>
                Deduction.Create(src.ConceptCode, src.Name, src.Application, src.Description),

            _ => throw new ArgumentException($"Invalid deduction configuration: Type={src.Type}")
        };
    }
}
