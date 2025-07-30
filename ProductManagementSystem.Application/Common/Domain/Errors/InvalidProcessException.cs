namespace ProductManagementSystem.Application.Common.Errors;

public class InvalidProcessException(string message) : Exception(message)
{
}