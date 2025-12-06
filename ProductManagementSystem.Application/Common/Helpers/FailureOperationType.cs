namespace ProductManagementSystem.Application.Common.Helpers;

public enum FailureOperationType
{
    None,
    Validation,
    NotFound,
    Unauthorized,
    Conflict,
    Error,
    UnknownFailure
}
