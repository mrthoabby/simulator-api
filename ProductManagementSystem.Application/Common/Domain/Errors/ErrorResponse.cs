namespace ProductManagementSystem.Application.Common.AppEntities.Errors;

public record ErrorResponse
{
    public required string Error { get; init; }
    public required string Message { get; init; }
    public required string Details { get; init; }

    public static ErrorResponse Create(string error, string message, string details)
    {
        return new ErrorResponse
        {
            Error = error,
            Message = message,
            Details = details
        };
    }
}