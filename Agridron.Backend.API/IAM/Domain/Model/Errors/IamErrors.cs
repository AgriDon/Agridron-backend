using Agridron.Backend.API.Shared.Domain.Model;

namespace Agridron.Backend.API.IAM.Domain.Model.Errors;

/// <summary>Catalog of expected domain and application errors for the IAM context.</summary>
public static class IamErrors
{
    public static readonly Error FullNameRequired =
        Error.Validation("UserFullNameRequired", "El nombre completo es obligatorio.");

    public static readonly Error FullNameTooLong =
        Error.Validation("UserFullNameTooLong", "El nombre completo no puede exceder los 150 caracteres.");

    public static readonly Error EmailRequired =
        Error.Validation("UserEmailRequired", "El correo electrónico es obligatorio.");

    public static readonly Error EmailTooLong =
        Error.Validation("UserEmailTooLong", "El correo electrónico no puede exceder los 255 caracteres.");

    public static readonly Error EmailInvalid =
        Error.Validation("UserEmailInvalid", "El formato del correo electrónico no es válido.");

    public static readonly Error PasswordRequired =
        Error.Validation("UserPasswordRequired", "La contraseña es obligatoria.");

    public static readonly Error PasswordTooShort =
        Error.Validation("UserPasswordTooShort", "La contraseña debe tener al menos 8 caracteres.");

    public static readonly Error PasswordTooLong =
        Error.Validation("UserPasswordTooLong", "La contraseña no puede exceder los 128 caracteres.");

    public static readonly Error PasswordHashRequired =
        Error.Validation("UserPasswordHashRequired", "El hash de la contraseña es obligatorio.");

    public static readonly Error PhoneNumberTooLong =
        Error.Validation("UserPhoneNumberTooLong", "El número de teléfono no puede exceder los 30 caracteres.");

    public static readonly Error UserIdInvalid =
        Error.Validation("UserIdInvalid", "El identificador de usuario debe ser mayor que cero.");

    public static readonly Error InvalidVerificationToken =
        Error.Validation("InvalidVerificationToken", "El enlace de verificación no es válido o ya expiró.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("InvalidCredentials", "Correo electrónico o contraseña incorrectos.");

    public static readonly Error UserInactive =
        Error.Forbidden("UserInactive", "La cuenta de usuario se encuentra inactiva.");

    public static readonly Error JwtSecretNotConfigured =
        Error.Unexpected("JwtSecretNotConfigured", "La clave de firma JWT no está configurada.");

    public static Error InvalidRole(string role) =>
        Error.Validation("UserRoleInvalid", $"El rol '{role}' no es válido.");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict("EmailAlreadyExists", $"Ya existe una cuenta registrada con el correo electrónico '{email}'.");

    public static Error UserNotFound(long id) =>
        Error.NotFound("UserNotFound", $"No se encontró el usuario con identificador {id}.");

    public static Error UserNotFoundByEmail(string email) =>
        Error.NotFound("UserNotFound", $"No se encontró un usuario registrado con el correo '{email}'.");

    public static Error ForbiddenProfileAccess(long targetUserId) =>
        Error.Forbidden(
            "ForbiddenProfileAccess",
            $"No tienes permisos para modificar al usuario con identificador {targetUserId}.");
}
