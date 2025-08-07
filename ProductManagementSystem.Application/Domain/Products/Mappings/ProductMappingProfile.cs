using AutoMapper;
using ProductManagementSystem.Application.Domain.Products.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.Products.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.Products.Models;
using ProductManagementSystem.Application.Domain.Shared.Type;
using ProductManagementSystem.Application.Domain.Shared.DTOs;


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
            .ConstructUsing(src => Provider.Create(src.Name, src.Url, new List<Offer>()));

        // Offer mappings
        CreateMap<Offer, OfferDTO>();
        CreateMap<CreateOfferDTO, Offer>()
            .ConstructUsing(src => Offer.Create(src.Url, Money.Create(src.Price.Value, src.Price.Currency), src.MinQuantity));

        // Competitor mappings
        CreateMap<Competitor, CompetitorDTO>();
        CreateMap<AddCompetitorDTO, Competitor>()
            .ConstructUsing(src => Competitor.Create(src.Url, Money.Create(src.Price.Value, src.Price.Currency), src.ImageUrl));

        // Deduction mappings
        CreateMap<Deduction, DeductionDTO>();
        CreateMap<AddDeductionDTO, Deduction>();
    }
}
