using Agridron.Backend.API.Shared.Domain.Model;

namespace Agridron.Backend.API.Shared.Domain.Model.Exceptions;

/// <summary>
///     Thrown when an aggregate or value object would be left in an invalid state.
/// </summary>
/// <remarks>
///     Domain exceptions are reserved for construction and invariant violations, where a
///     <c>Result</c> cannot be returned (for example, a constructor). Expected use-case failures
///     are still modelled with <see cref="Result{T}" />.
/// </remarks>
public sealed class DomainException : Exception
{
    /// <summary>
    ///     Creates a domain exception from a domain error.
    /// </summary>
    /// <param name="error">The error that describes the violated rule.</param>
    public DomainException(Error error) : base(error?.Message)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }

    /// <summary>
    ///     Creates a domain exception from a domain error and an inner exception.
    /// </summary>
    /// <param name="error">The error that describes the violated rule.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public DomainException(Error error, Exception innerException) : base(error?.Message, innerException)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }

    /// <summary>The domain error that describes the violated rule.</summary>
    public Error Error { get; }
}
