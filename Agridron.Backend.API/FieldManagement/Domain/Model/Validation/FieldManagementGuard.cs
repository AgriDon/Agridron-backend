using Agridron.Backend.API.Shared.Domain.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Validation;

/// <summary>
///     Validation helpers shared by the Field Management aggregates.
/// </summary>
/// <remarks>
///     They throw <see cref="DomainException" /> because they run inside constructors and
///     business methods, where returning a <c>Result</c> is not possible.
/// </remarks>
internal static class FieldManagementGuard
{
    /// <summary>
    ///     Ensures a text value is present and within the allowed length, and returns it trimmed.
    /// </summary>
    public static string RequiredText(string? value, Error missing, int maxLength, Error tooLong)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(missing);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException(tooLong);
        }

        return trimmed;
    }

    /// <summary>
    ///     Ensures an optional URL is absolute, uses http or https, and fits in the column.
    /// </summary>
    public static string? OptionalUrl(string? value, int maxLength, Error invalid)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException(invalid);
        }

        var isValid = Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
                      && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        if (!isValid)
        {
            throw new DomainException(invalid);
        }

        return trimmed;
    }

    /// <summary>
    ///     Ensures an external identifier is positive.
    /// </summary>
    public static long RequiredId(long value, Error invalid)
    {
        if (value <= 0)
        {
            throw new DomainException(invalid);
        }

        return value;
    }
}
