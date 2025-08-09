namespace ProductManagementSystem.Application.Common.Domain.Errors;

public class ApplicationError : Exception
{
    public ApplicationError(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}