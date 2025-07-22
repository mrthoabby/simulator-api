using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Application.Products.Commands.CreateProduct;
using ProductManagementSystem.Application.Products.Controllers.DTOs.Request;

namespace ProductManagementSystem.Application.Products.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreateProductController : ControllerBase
{
    private readonly CreateProductHandler _handler;

    public CreateProductController(CreateProductHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
    {
        try
        {
            var command = request.ToCommand();
            var result = await _handler.Handle(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}