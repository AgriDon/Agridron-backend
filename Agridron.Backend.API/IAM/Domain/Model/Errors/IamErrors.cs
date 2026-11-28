using Agridron.Backend.API.Shared.Domain.Model;

namespace Agridron.Backend.API.IAM.Domain.Model.Errors;

/// <summary>Catalog of expected domain and application errors for the IAM context.</summary>
public static class IamErrors
{
    public static readonly Error FullNameRequired =
        Error.Validation("UserFullNameRequired", "The user full name is required.");

    public static readonly Error FullNameTooLong =
        Error.Validation("UserFullNameTooLong", "The user full name cannot exceed 150 characters.");

    public static readonly Error EmailRequired =
        Error.Validation("UserEmailRequired", "The email address is required.");

    public static readonly Error EmailTooLong =
        Error.Validation("UserEmailTooLong", "The email address cannot exceed 255 characters.");

    public static readonly Error EmailInvalid =
        Error.Validation("UserEmailInvalid", "The email address format is invalid.");

    public static readonly Error PasswordRequired =
        Error.Validation("UserPasswordRequired", "The password is required.");

    public static readonly Error PasswordTooShort =
        Error.Validation("UserPasswordTooShort", "The password must be at least 8 characters long.");

    public static readonly Error PasswordTooLong =
        Error.Validation("UserPasswordTooLong", "The password cannot exceed 128 characters.");

    public static readonly Error PasswordHashRequired =
        Error.Validation("UserPasswordHashRequired", "The password hash is required.");

    public static readonly Error PhoneNumberTooLong =
        Error.Validation("UserPhoneNumberTooLong", "The phone number cannot exceed 30 characters.");

    public static readonly Error UserIdInvalid =
        Error.Validation("UserIdInvalid", "The user identifier must be greater than zero.");

    public static readonly Error InvalidVerificationToken =
        Error.Validation("InvalidVerificationToken", "The email verification token is invalid or has already been used.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("InvalidCredentials", "Invalid email or password.");

    public static readonly Error UserInactive =
        Error.Forbidden("UserInactive", "The user account is inactive.");

    public static readonly Error JwtSecretNotConfigured =
        Error.Unexpected("JwtSecretNotConfigured", "JWT signing key is not configured.");

    public static Error InvalidRole(string role) =>
        Error.Validation("UserRoleInvalid", $"The role '{role}' is not valid.");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict("EmailAlreadyExists", $"A user with email '{email}' already exists.");

    public static Error UserNotFound(long id) =>
        Error.NotFound("UserNotFound", $"User with id {id} was not found.");

    public static Error UserNotFoundByEmail(string email) =>
        Error.NotFound("UserNotFound", $"User with email '{email}' was not found.");

    public static Error ForbiddenProfileAccess(long targetUserId) =>
        Error.Forbidden(
            "ForbiddenProfileAccess",
            $"You do not have permission to modify user with id {targetUserId}.");
}
