namespace ProductManagementSystem.Application.Common.Errors;

public class NotFoundException(string message) : Exception(message);