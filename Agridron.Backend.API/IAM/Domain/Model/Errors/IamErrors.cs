using Agridron.Backend.API.Shared.Domain.Model;

namespace Agridron.Backend.API.IAM.Domain.Model.Errors;

/// <summary>
///     Catalog of domain and application errors for the IAM bounded context.
/// </summary>
/// <remarks>
///     Design Pattern: Factory Method — exposes standardized factory methods and constants that
///     produce strongly categorized <see cref="Error" /> instances mapped consistently to RFC 7807 Problem Details.
/// </remarks>
public static class IamErrors
{
    /// <summary>Error returned when the user's full name is empty or whitespace.</summary>
    public static readonly Error FullNameRequired =
        Error.Validation("UserFullNameRequired", "The user full name is required.");

    /// <summary>Error returned when the user's full name exceeds the maximum length.</summary>
    public static readonly Error FullNameTooLong =
        Error.Validation("UserFullNameTooLong", "The user full name cannot exceed 150 characters.");

    /// <summary>Error returned when the email address is missing.</summary>
    public static readonly Error EmailRequired =
        Error.Validation("UserEmailRequired", "The email address is required.");

    /// <summary>Error returned when the email address exceeds the maximum length.</summary>
    public static readonly Error EmailTooLong =
        Error.Validation("UserEmailTooLong", "The email address cannot exceed 255 characters.");

    /// <summary>Error returned when the email address format is invalid.</summary>
    public static readonly Error EmailInvalid =
        Error.Validation("UserEmailInvalid", "The email address format is invalid.");

    /// <summary>Error returned when a raw password is missing.</summary>
    public static readonly Error PasswordRequired =
        Error.Validation("UserPasswordRequired", "The password is required.");

    /// <summary>Error returned when a raw password is shorter than the minimum allowed length.</summary>
    public static readonly Error PasswordTooShort =
        Error.Validation("UserPasswordTooShort", "The password must be at least 8 characters long.");

    /// <summary>Error returned when a raw password exceeds the maximum allowed length.</summary>
    public static readonly Error PasswordTooLong =
        Error.Validation("UserPasswordTooLong", "The password cannot exceed 128 characters.");

    /// <summary>Error returned when a password hash is missing or invalid.</summary>
    public static readonly Error PasswordHashRequired =
        Error.Validation("UserPasswordHashRequired", "The password hash is required.");

    /// <summary>Error returned when the optional phone number exceeds the maximum allowed length.</summary>
    public static readonly Error PhoneNumberTooLong =
        Error.Validation("UserPhoneNumberTooLong", "The phone number cannot exceed 30 characters.");

    /// <summary>Error returned when a user identifier is not positive.</summary>
    public static readonly Error UserIdInvalid =
        Error.Validation("UserIdInvalid", "The user identifier must be greater than zero.");

    /// <summary>Error returned when an email verification token is missing, invalid, or already consumed.</summary>
    public static readonly Error InvalidVerificationToken =
        Error.Validation("InvalidVerificationToken", "The email verification token is invalid or has already been used.");

    /// <summary>Error returned when authentication credentials are invalid (401 Unauthorized).</summary>
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("InvalidCredentials", "Invalid email or password.");

    /// <summary>Error returned when an inactive account attempts to sign in (403 Forbidden).</summary>
    public static readonly Error UserInactive =
        Error.Forbidden("UserInactive", "The user account is inactive.");

    /// <summary>Error returned when the JWT signing secret is missing or shorter than 32 bytes.</summary>
    public static readonly Error JwtSecretNotConfigured =
        Error.Unexpected("JwtSecretNotConfigured", "JWT signing key is not configured.");

    /// <summary>Creates a validation error for an unsupported user role.</summary>
    /// <param name="role">The unrecognized role string.</param>
    /// <returns>A validation <see cref="Error" />.</returns>
    public static Error InvalidRole(string role)
    {
        return Error.Validation("UserRoleInvalid", $"The role '{role}' is not valid.");
    }

    /// <summary>Creates a conflict error when an email address is already registered.</summary>
    /// <param name="email">The duplicate normalized email address.</param>
    /// <returns>A conflict <see cref="Error" />.</returns>
    public static Error EmailAlreadyExists(string email)
    {
        return Error.Conflict("EmailAlreadyExists", $"A user with email '{email}' already exists.");
    }

    /// <summary>Creates a not-found error when a user identifier does not exist.</summary>
    /// <param name="id">The missing user identifier.</param>
    /// <returns>A not-found <see cref="Error" />.</returns>
    public static Error UserNotFound(long id)
    {
        return Error.NotFound("UserNotFound", $"User with id {id} was not found.");
    }

    /// <summary>Creates a not-found error when a user email address does not exist.</summary>
    /// <param name="email">The missing user email address.</param>
    /// <returns>A not-found <see cref="Error" />.</returns>
    public static Error UserNotFoundByEmail(string email)
    {
        return Error.NotFound("UserNotFound", $"User with email '{email}' was not found.");
    }

    /// <summary>Creates a forbidden error when a caller attempts to modify another user's profile without admin privileges.</summary>
    /// <param name="targetUserId">The target user identifier.</param>
    /// <returns>A forbidden <see cref="Error" />.</returns>
    public static Error ForbiddenProfileAccess(long targetUserId)
    {
        return Error.Forbidden(
            "ForbiddenProfileAccess",
            $"You do not have permission to modify user with id {targetUserId}.");
    }
}
