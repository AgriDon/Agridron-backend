namespace Agridron.Backend.API.Shared.Domain.Model;

/// <summary>
///     Represents an expected domain error.
/// </summary>
/// <param name="Code">Machine-readable code, unique inside the bounded context.</param>
/// <param name="Message">Human-readable message.</param>
/// <param name="Type">The error category used to derive the HTTP status.</param>
/// <param name="Details">Optional extra context for diagnostics.</param>
public record Error(
    string Code,
    string Message,
    ErrorType Type = ErrorType.Unexpected,
    string? Details = null)
{
    /// <summary>No error.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Unexpected);

    /// <summary>A null value was provided where one was required.</summary>
    public static readonly Error NullValue = new(
        "NULL_VALUE",
        "The specified result value is null.",
        ErrorType.Validation);

    /// <summary>Creates a validation error.</summary>
    public static Error Validation(string code, string message, string? details = null)
    {
        return new Error(code, message, ErrorType.Validation, details);
    }

    /// <summary>Creates a not-found error.</summary>
    public static Error NotFound(string code, string message, string? details = null)
    {
        return new Error(code, message, ErrorType.NotFound, details);
    }

    /// <summary>Creates a conflict error.</summary>
    public static Error Conflict(string code, string message, string? details = null)
    {
        return new Error(code, message, ErrorType.Conflict, details);
    }

    /// <summary>Creates a business rule violation error.</summary>
    public static Error BusinessRule(string code, string message, string? details = null)
    {
        return new Error(code, message, ErrorType.BusinessRule, details);
    }

    /// <summary>Creates an unexpected error.</summary>
    public static Error Unexpected(string code, string message, string? details = null)
    {
        return new Error(code, message, ErrorType.Unexpected, details);
    }
}
