namespace ProductManagementSystem.Application.Common.AppEntities.Errors;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}