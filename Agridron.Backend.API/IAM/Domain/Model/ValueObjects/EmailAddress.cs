using System.Net.Mail;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.IAM.Domain.Model.ValueObjects;

/// <summary>Normalized, lowercase email address value object.</summary>
public sealed class EmailAddress : ValueObject
{
    public const int MaxLength = 255;

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

    public string Value { get; }

    /// <summary>Validates and constructs an <see cref="EmailAddress" /> from a raw string.</summary>
    public static EmailAddress Create(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

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
