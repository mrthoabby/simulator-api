using System.ComponentModel.DataAnnotations;
using ProductManagementSystem.Application.Common.Domain.Enum;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Products.Domain.Type;
using ProductManagementSystem.Application.Products.Models.Entity;

namespace ProductManagementSystem.Application.Products.Commands.CreateProduct;

public class CreateProductCommand
{
    public string Name { get; private set; }
    public Price Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public List<Deduction>? Deductions { get; private set; }
    public List<Provider>? Providers { get; private set; }
    public List<Competitor>? Competitors { get; private set; }

    private CreateProductCommand()
    {
        Name = string.Empty;
        Price = new Price(0, EnumCurrency.USD);
    }

    public Product ToProduct()
    {
        var builder = Product.Create(Name, Price);

        if (ImageUrl != null)
            builder.WithImageUrl(ImageUrl);
        if (Deductions != null)
            builder.WithDeductions(Deductions);
        if (Providers != null)
            builder.WithProviders(Providers);
        if (Competitors != null)
            builder.WithCompetitors(Competitors);

        return builder.Build();
    }


    public class CreateCommandBuilder
    {
        private readonly CreateProductCommand _productCommand = new();
        private readonly CreateProductValidator _createProductValidator;

        public CreateCommandBuilder(CreateProductValidator createProductValidator)
        {
            _createProductValidator = createProductValidator;
        }

        public CreateCommandBuilder WithName(string name)
        {
            _productCommand.Name = name;
            return this;
        }

        public CreateCommandBuilder WithPrice(Price price)
        {
            _productCommand.Price = price;
            return this;
        }

        public CreateCommandBuilder WithImageUrl(string imageUrl)
        {
            _productCommand.ImageUrl = imageUrl;
            return this;
        }

        public CreateCommandBuilder WithDeductions(List<Deduction> deductions)
        {
            _productCommand.Deductions = deductions;
            return this;
        }

        public CreateCommandBuilder WithProviders(List<Provider> providers)
        {
            _productCommand.Providers = providers;
            return this;
        }

        public CreateCommandBuilder WithCompetitors(List<Competitor> competitors)
        {
            _productCommand.Competitors = competitors;
            return this;
        }

        public CreateProductCommand Build()
        {
            var validationResult = _createProductValidator.Validate(_productCommand);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault());

            return _productCommand;
        }
    }
}