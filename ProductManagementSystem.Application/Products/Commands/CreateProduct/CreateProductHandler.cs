using ProductManagementSystem.Application.Common;
using ProductManagementSystem.Application.Products.Models.Entity;
using ProductManagementSystem.Application.Products.Repository;

namespace ProductManagementSystem.Application.Products.Commands.CreateProduct;

public class CreateProductHandler(IProductRepository productRepository)
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<Result<Product>> Handle(CreateProductCommand command)
    {
        try
        {
            var product = command.ToProduct();
            var createdProduct = await _productRepository.CreateAsync(product);
            return Result<Product>.Success(createdProduct, "Product created successfully");
        }
        catch (Exception ex)
        {
            return Result<Product>.Failure($"Failed to create product: {ex.Message}");
        }
    }
}