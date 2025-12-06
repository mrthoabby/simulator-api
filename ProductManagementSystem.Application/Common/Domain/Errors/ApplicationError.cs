namespace ProductManagementSystem.Application.Common.AppEntities.Errors;

public class ApplicationError : Exception
{
    public ApplicationError(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}