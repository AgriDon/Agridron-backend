using System.Net.Mail;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Validated, normalized (lowercase) email address value object.
/// </summary>
/// <remarks>
///     Design Pattern: Factory Method (<see cref="Create" />) combined with the DDD Value Object pattern
///     to guarantee that only valid, normalized email addresses exist in the domain model.
/// </remarks>
public sealed class EmailAddress : ValueObject
{
    /// <summary>Maximum allowed length of an email address.</summary>
    public const int MaxLength = 255;

    /// <summary>
    ///     Initializes a new validated and normalized <see cref="EmailAddress" /> instance.
    /// </summary>
    /// <param name="value">The raw email address string.</param>
    /// <exception cref="DomainException">Thrown when the email address is missing, exceeds <see cref="MaxLength" />, or has an invalid format.</exception>
    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(IamErrors.EmailRequired);
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > MaxLength)
        {
            throw new DomainException(IamErrors.EmailTooLong);
        }

        if (!IsValidFormat(normalized))
        {
            throw new DomainException(IamErrors.EmailInvalid);
        }

        Value = normalized;
    }

    /// <summary>Gets the normalized lowercase email address.</summary>
    public string Value { get; }

    /// <summary>
    ///     Factory method that creates a validated <see cref="EmailAddress" /> from a raw string.
    /// </summary>
    /// <param name="value">The raw email address string.</param>
    /// <returns>A validated <see cref="EmailAddress" /> instance.</returns>
    public static EmailAddress Create(string value) => new(value);

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    private static bool IsValidFormat(string email)
    {
        if (email.Any(char.IsWhiteSpace))
        {
            return false;
        }

        try
        {
            var address = new MailAddress(email);
            if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
            {
                return false;
            }

            var domain = email[(atIndex + 1)..];
            return domain.Contains('.', StringComparison.Ordinal)
                   && !domain.StartsWith('.')
                   && !domain.EndsWith('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
