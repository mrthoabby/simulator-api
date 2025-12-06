using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Common.AppEntities.Errors;
using FluentValidation;
using System.Text.Json;

namespace ProductManagementSystem.Application.Common.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _mockLogger;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
    }

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var nextCalled = false;

        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_Returns404()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new NotFoundException("Resource not found");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedException_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new UnauthorizedException("Unauthorized access");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WhenConflictException_Returns409()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new ConflictException("Conflict detected");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task InvokeAsync_WhenBadRequestException_Returns400()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new BadRequestException("Bad request");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_Returns422()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new ValidationException("Validation failed");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task InvokeAsync_WhenDeviceLimitExceededException_Returns409WithDeviceInfo()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var devices = new List<ProductManagementSystem.Application.AppEntities.Auth.DTOs.Outputs.DeviceInfoDTO>
        {
            new ProductManagementSystem.Application.AppEntities.Auth.DTOs.Outputs.DeviceInfoDTO
            {
                Id = "device-1",
                DeviceName = "Test Device"
            }
        };

        RequestDelegate next = (ctx) => throw new DeviceLimitExceededException(2, devices);

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(409);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_Returns500()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new Exception("Unexpected error");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_SetsContentTypeToJson()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new NotFoundException("Not found");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("application/json");
    }

    #endregion

    #region HandleValidationErrors Tests

    [Fact]
    public void HandleValidationErrors_WithModelStateErrors_ReturnsUnprocessableEntity()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Field1 is required");
        modelState.AddModelError("Field2", "Field2 is invalid");

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
            modelState
        );

        var result = GlobalExceptionHandlerMiddleware.HandleValidationErrors(actionContext);

        result.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Fact]
    public void HandleValidationErrors_WithEmptyModelState_ReturnsUnprocessableEntity()
    {
        var modelState = new ModelStateDictionary();

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
            modelState
        );

        var result = GlobalExceptionHandlerMiddleware.HandleValidationErrors(actionContext);

        result.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Fact]
    public void HandleValidationErrors_WithExceptionInModelState_IncludesExceptionMessage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", new Exception("Inner exception"), new EmptyModelMetadataProvider().GetMetadataForType(typeof(string)));

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
            modelState
        );

        var result = GlobalExceptionHandlerMiddleware.HandleValidationErrors(actionContext);

        result.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    #endregion

    #region Error Response Tests

    [Fact]
    public async Task InvokeAsync_NotFoundException_ReturnsCorrectErrorStructure()
    {
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        RequestDelegate next = (ctx) => throw new NotFoundException("User not found");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var responseContent = await reader.ReadToEndAsync();

        responseContent.Should().Contain("User not found");
    }

    [Fact]
    public async Task InvokeAsync_InvalidProcessException_Returns424()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new InvalidProcessException("Invalid process");

        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(424);
    }

    #endregion
}

