namespace ProductManagementSystem.Application.Common.Domain.Errors;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}