namespace ProductManagementSystem.Application.Common.Errors;

public class ConflictException(string message) : Exception(message);