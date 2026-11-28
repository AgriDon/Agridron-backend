using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.IAM.Domain.Model.Validation;

internal static class IamGuard
{
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

    public static long RequiredId(long value, Error invalid)
    {
        if (value <= 0)
        {
            throw new DomainException(invalid);
        }

        return value;
    }

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
