using System.Net;
using System.Text.Json;
using ProductManagementSystem.Application.Common.Domain.Errors;
using ProductManagementSystem.Application.Common.Errors;
using FluentValidation;

namespace ProductManagementSystem.Application.Common.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    public static Microsoft.AspNetCore.Mvc.IActionResult HandleValidationErrors(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        var errors = new List<string>();

        foreach (var kvp in context.ModelState.Where(x => x.Value?.Errors.Count > 0))
        {
            foreach (var error in kvp.Value?.Errors ?? [])
            {
                if (!string.IsNullOrEmpty(error.ErrorMessage))
                {
                    errors.Add(error.ErrorMessage);
                }
                else if (error.Exception?.Message != null)
                {
                    errors.Add(error.Exception.Message);
                }
            }
        }

        var errorMessage = string.Join("; ", errors);

        var errorResponse = CreateErrorResponseForValidation(errorMessage);

        return new Microsoft.AspNetCore.Mvc.UnprocessableEntityObjectResult(errorResponse);
    }

    private static ErrorResponse CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException ex => ErrorResponse.Create(
                GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.ValidationFailed,
                ex.Message,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.ValidationFailed),

            NotFoundException ex => ErrorResponse.Create(
                GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.UserNotFound,
                ex.Message,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.UserNotFound),

            UnauthorizedException ex => ErrorResponse.Create(
                GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.Unauthorized,
                ex.Message,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.Unauthorized),

            ConflictException ex => ErrorResponse.Create(
                GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.Conflict,
                ex.Message,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.Conflict),

            BadRequestException ex => ErrorResponse.Create(
                GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.BadRequest,
                ex.Message,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.BadRequest),

            InvalidProcessException ex => ErrorResponse.Create(
                GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.InvalidProcess,
                ex.Message,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.InvalidProcess),

            _ => ErrorResponse.Create(
                GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.InternalServer,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.InternalServer,
                GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.InternalServerDetails)
        };
    }

    private static ErrorResponse CreateErrorResponseForValidation(string message)
    {
        return ErrorResponse.Create(
            GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.ValidationFailed,
            message,
            GlobalExceptionHandlerMiddlewareExtensionsValues.Messages.ValidationFailed);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is DeviceLimitExceededException deviceLimitEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = GlobalExceptionHandlerMiddlewareExtensionsValues.Values.ApplicationJson;

            var deviceLimitResponse = new
            {
                error = GlobalExceptionHandlerMiddlewareExtensionsValues.Errors.DeviceLimitExceeded,
                message = deviceLimitEx.Message,
                maxDevices = deviceLimitEx.MaxDevices,
                activeDevices = deviceLimitEx.ActiveDevices
            };

            var deviceLimitJson = JsonSerializer.Serialize(deviceLimitResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(deviceLimitJson);
            return;
        }

        var (statusCode, errorResponse) = exception switch
        {
            NotFoundException ex => (HttpStatusCode.NotFound, CreateErrorResponse(ex)),
            UnauthorizedException ex => (HttpStatusCode.Unauthorized, CreateErrorResponse(ex)),
            InvalidProcessException ex => (HttpStatusCode.FailedDependency, CreateErrorResponse(ex)),
            ValidationException ex => (HttpStatusCode.UnprocessableEntity, CreateErrorResponse(ex)),
            ConflictException ex => (HttpStatusCode.Conflict, CreateErrorResponse(ex)),
            BadRequestException ex => (HttpStatusCode.BadRequest, CreateErrorResponse(ex)),
            _ => (HttpStatusCode.InternalServerError, CreateErrorResponse(exception))
        };

        _logger.LogError(exception, "Unexpected error: {ErrorMessage}", exception.Message);

        await WriteErrorResponseAsync(context, (int)statusCode, errorResponse);
    }

    private async Task WriteErrorResponseAsync(HttpContext context, int statusCode, ErrorResponse errorResponse)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = GlobalExceptionHandlerMiddlewareExtensionsValues.Values.ApplicationJson;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public static class GlobalExceptionHandlerMiddlewareExtensionsValues
{
    public static class Errors
    {
        public const string ValidationFailed = "Validation failed";
        public const string Conflict = "Conflict";
        public const string UserNotFound = "User not found";
        public const string Unauthorized = "Unauthorized";
        public const string BadRequest = "Bad Request";
        public const string DeviceLimitExceeded = "Device limit exceeded";
        public const string InvalidProcess = "Invalid process";
        public const string InternalServer = "Internal server error";
    }

    public static class Messages
    {
        public const string ValidationFailed = "Please check the provided data and correct the validation errors.";
        public const string Conflict = "The request could not be completed due to a conflict with the current state of the resource.";
        public const string UserNotFound = "The requested resource does not exist. Please verify the provided information.";
        public const string Unauthorized = "Invalid credentials or insufficient permissions.";
        public const string BadRequest = "The request was invalid or malformed.";
        public const string DeviceLimitExceeded = "The device limit has been exceeded. Please contact support if you need more devices.";
        public const string InvalidProcess = "Please check the provided data and try again.";
        public const string InternalServer = "An unexpected error occurred while processing your request.";
        public const string InternalServerDetails = "Please try again later or contact support if the problem persists.";
    }
    public static class Values
    {
        public const string ApplicationJson = "application/json";
    }
}