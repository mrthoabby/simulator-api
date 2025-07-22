using FluentAssertions;
using Moq;
using ProductManagementSystem.Application.Products.Models.Entity;
using ProductManagementSystem.Application.Products.Repository;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Common.Domain.Enum;
using ProductManagementSystem.Application.Products.Domain.Type;
using Xunit;
using static ProductManagementSystem.Application.Products.Commands.CreateProduct.CreateProductCommand;
using System.ComponentModel.DataAnnotations;


namespace ProductManagementSystem.Application.Products.Commands.CreateProduct.Tests;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly CreateCommandBuilder createCommandBuilder;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        createCommandBuilder = new CreateCommandBuilder(new CreateProductValidator());
        _handler = new CreateProductHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("Test Product")
            .WithPrice(new Price(100.50m, EnumCurrency.USD))
            .WithImageUrl("https://example.com/image.jpg")
            .WithDeductions(
                new List<Deduction> {
                    new("Test Deduction", "Test Deduction Description", "Test Deduction Concept Code",
                        new DeductionValue(10.00m, EnumDeductionType.PERCENTAGE),
                        EnumDeductionApplication.ADD_UNIT_PRODUCT_PRICE)
                }
            )
            .WithProviders(
                new List<Provider> {
                    new Provider("Test Provider", "https://provider.com", new List<Offer> { new Offer("https://provider.com", new Price(10.00m, EnumCurrency.USD), 10) })
                }
            )
            .WithCompetitors(
                new List<Competitor> {
                    new Competitor("https://competitor.com", "https://competitor.com",  new Price(90.00m, EnumCurrency.USD))
                }
            )
            .Build();

        var product = command.ToProduct();

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(product);


        // Act
        var _result = await _handler.Handle(command);


        // Assert
        _result.Should().NotBeNull();
        _result.Should().Be(product);

        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public void Handle_EmptyProductName_ShouldFailValidation()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("")
            .WithPrice(new Price(100.50m, EnumCurrency.USD))
            .WithImageUrl("https://example.com/image.jpg")
            .WithDeductions(new List<Deduction>())
            .WithProviders(new List<Provider>())
            .WithCompetitors(new List<Competitor>());

        // Act & Assert
        var action = () => command.Build();

        // Assert
        action.Should().Throw<ValidationException>()
            .WithMessage(CreateProductValidationMessages.PRODUCT_NAME_REQUIRED);
    }

    [Fact]
    public void Handle_NegativePrice_ShouldFailValidation()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("Test Product")
            .WithPrice(new Price(-50.00m, EnumCurrency.USD))
            .WithImageUrl("https://example.com/image.jpg")
            .WithDeductions(new List<Deduction>())
            .WithProviders(new List<Provider>())
            .WithCompetitors(new List<Competitor>());

        // Act
        var action = () => command.Build();

        // Assert
        action.Should().Throw<ValidationException>()
            .WithMessage(CreateProductValidationMessages.PRICE_VALUE_GREATER_THAN_0);
    }

    [Fact]
    public void Handle_InvalidImageUrl_ShouldFailValidation()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("Test Product")
            .WithPrice(new Price(100.50m, EnumCurrency.USD))
            .WithImageUrl("invalid-url")
            .WithDeductions(new List<Deduction>())
            .WithProviders(new List<Provider>())
            .WithCompetitors(new List<Competitor>());

        // Act
        var action = () => command.Build();

        // Assert
        action.Should().Throw<ValidationException>()
            .WithMessage(CreateProductValidationMessages.IMAGE_URL_VALID);
    }

    [Fact]
    public void Handle_NegativeDeductionValue_ShouldFailValidation()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("Test Product")
            .WithPrice(new Price(100.50m, EnumCurrency.USD))
            .WithImageUrl("https://example.com/image.jpg")
            .WithDeductions(new List<Deduction> {
                new("Test Deduction", "Test Deduction Description", "Test Deduction Concept Code",
                    new DeductionValue(-10.00m, EnumDeductionType.PERCENTAGE),
                    EnumDeductionApplication.ADD_UNIT_PRODUCT_PRICE)
            })
            .WithProviders(new List<Provider>())
            .WithCompetitors(new List<Competitor> {
                new Competitor("https://competitor.com", "https://competitor.com",  new Price(90.00m, EnumCurrency.USD))
            });

        // Act
        var action = () => command.Build();

        // Assert
        action.Should().Throw<ValidationException>()
            .WithMessage(CreateProductValidationMessages.DEDUCTION_VALUE_GREATER_THAN_0);
    }

    [Fact]
    public void Handle_InvalidProviderUrl_ShouldFailValidation()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("Test Product")
            .WithPrice(new Price(100.50m, EnumCurrency.USD))
            .WithImageUrl("https://example.com/image.jpg")
            .WithDeductions(new List<Deduction>())
            .WithProviders(new List<Provider> {
                new Provider("Test Provider", "invalid-url", new List<Offer>())
            })
            .WithCompetitors(new List<Competitor>());

        // Act
        var action = () => command.Build();

        // Assert
        action.Should().Throw<ValidationException>()
            .WithMessage(CreateProductValidationMessages.PROVIDER_URL_VALID);
    }

    [Fact]
    public void Handle_InvalidCompetitorPrice_ShouldFailValidation()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("Test Product")
            .WithPrice(new Price(100.50m, EnumCurrency.USD))
            .WithImageUrl("https://example.com/image.jpg")
            .WithDeductions(new List<Deduction>())
            .WithProviders(new List<Provider>())
            .WithCompetitors(new List<Competitor> {
                new Competitor("https://competitor.com", "https://competitor.com", new Price(-10.00m, EnumCurrency.USD))
            });

        // Act
        var action = () => command.Build();

        // Assert
        action.Should().Throw<ValidationException>()
            .WithMessage(CreateProductValidationMessages.COMPETITOR_PRICE_GREATER_THAN_0);
    }

    [Fact]
    public void Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var command = createCommandBuilder
            .WithName("Test Product")
            .WithPrice(new Price(100.50m, EnumCurrency.USD))
            .WithImageUrl("https://example.com/image.jpg")
            .WithDeductions(new List<Deduction>())
            .WithProviders(new List<Provider>())
            .WithCompetitors(new List<Competitor>());

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var action = () => command.Build();

        // Assert
        action.Should().Throw<Exception>()
            .WithMessage("Database error");
    }
}