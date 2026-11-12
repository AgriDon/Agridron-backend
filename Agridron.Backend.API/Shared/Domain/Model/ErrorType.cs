namespace Agridron.Backend.API.Shared.Domain.Model;

/// <summary>
///     Classifies a domain error so the presentation layer can map it to an HTTP status
///     without the domain knowing anything about HTTP.
/// </summary>
public enum ErrorType
{
    /// <summary>Invalid input or malformed data.</summary>
    Validation = 0,

    /// <summary>The requested resource does not exist.</summary>
    NotFound = 1,

    /// <summary>The operation conflicts with the current state.</summary>
    Conflict = 2,

    /// <summary>A business rule was violated.</summary>
    BusinessRule = 3,

    /// <summary>An unexpected failure.</summary>
    Unexpected = 4
}
