using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.IAM.Domain.Model.Validation;

/// <summary>
///     Validation helpers shared across the IAM domain model.
/// </summary>
internal static class IamGuard
{
    /// <summary>
    ///     Ensures a required text value is non-blank and within the maximum allowed length, returning it trimmed.
    /// </summary>
    /// <param name="value">The raw text value to validate.</param>
    /// <param name="missing">The domain error thrown when the value is null or whitespace.</param>
    /// <param name="maxLength">The maximum permitted trimmed character length.</param>
    /// <param name="tooLong">The domain error thrown when the trimmed value exceeds <paramref name="maxLength" />.</param>
    /// <returns>The validated trimmed string.</returns>
    /// <exception cref="DomainException">Thrown when validation fails.</exception>
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
    ///     Validates an optional text value, returning <c>null</c> when blank or the trimmed value when valid.
    /// </summary>
    /// <param name="value">The optional raw text value.</param>
    /// <param name="maxLength">The maximum permitted trimmed character length.</param>
    /// <param name="tooLong">The domain error thrown when the trimmed value exceeds <paramref name="maxLength" />.</param>
    /// <returns>The trimmed string, or <c>null</c> if the input is blank.</returns>
    /// <exception cref="DomainException">Thrown when the value exceeds <paramref name="maxLength" />.</exception>
    public static string? OptionalText(string? value, int maxLength, Error tooLong)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException(tooLong);
        }

        return trimmed;
    }

    /// <summary>
    ///     Ensures an identifier is strictly positive.
    /// </summary>
    /// <param name="value">The identifier value to validate.</param>
    /// <param name="invalid">The domain error thrown when the value is zero or negative.</param>
    /// <returns>The validated positive identifier.</returns>
    /// <exception cref="DomainException">Thrown when <paramref name="value" /> is less than or equal to zero.</exception>
    public static long RequiredId(long value, Error invalid)
    {
        if (value <= 0)
        {
            throw new DomainException(invalid);
        }

        return value;
    }

    /// <summary>
    ///     Validates raw password length constraints prior to hashing.
    /// </summary>
    /// <param name="password">The raw password provided by the caller.</param>
    /// <param name="minLength">The minimum required character length.</param>
    /// <param name="maxLength">The maximum permitted character length.</param>
    /// <exception cref="DomainException">Thrown when the password is missing, too short, or too long.</exception>
    public static void ValidateRawPassword(string? password, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainException(IamErrors.PasswordRequired);
        }

        if (password.Length < minLength)
        {
            throw new DomainException(IamErrors.PasswordTooShort);
        }

        if (password.Length > maxLength)
        {
            throw new DomainException(IamErrors.PasswordTooLong);
        }
    }
}
