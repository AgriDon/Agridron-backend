using System.Diagnostics.CodeAnalysis;
using Agridron.Backend.API.Shared.Domain.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Validation;

/// <summary>
///     Guard clauses for invariant validation across Flight Operations.
/// </summary>
public static class FlightOperationsGuard
{
    public static string RequiredText(
        [NotNull] string? value,
        Error requiredError,
        int maxLength,
        Error tooLongError)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(requiredError);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException(tooLongError);
        }

        return trimmed;
    }

    public static string? OptionalText(
        string? value,
        int maxLength,
        Error tooLongError)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException(tooLongError);
        }

        return trimmed;
    }

    public static string? OptionalUrl(
        string? value,
        int maxLength,
        Error invalidError)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException(invalidError);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException(invalidError);
        }

        return trimmed;
    }
}
