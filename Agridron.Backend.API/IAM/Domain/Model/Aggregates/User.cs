using System.Security.Cryptography;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.IAM.Domain.Model.Events;
using Agridron.Backend.API.IAM.Domain.Model.Validation;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;

namespace Agridron.Backend.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     Aggregate root representing a user account registered on the Agridron platform.
/// </summary>
/// <remarks>
///     Design Pattern: Factory Method (<see cref="Register" />) — encapsulates valid user creation,
///     default lifecycle state, and secure verification token generation inside the aggregate boundary.
/// </remarks>
public sealed class User : AggregateRoot<UserId>, IAuditableEntity
{
    /// <summary>Maximum length of the user's full name.</summary>
    public const int MaxFullNameLength = 150;

    /// <summary>Maximum length of the user's email address.</summary>
    public const int MaxEmailLength = EmailAddress.MaxLength;

    /// <summary>Maximum length of the stored password hash.</summary>
    public const int MaxPasswordHashLength = 255;

    /// <summary>Maximum length of the optional phone number.</summary>
    public const int MaxPhoneNumberLength = 30;

    /// <summary>Maximum length of the URL-safe email verification token.</summary>
    public const int MaxVerificationTokenLength = 128;

    /// <summary>Minimum length of a raw user password.</summary>
    public const int MinPasswordLength = 8;

    /// <summary>Maximum length of a raw user password.</summary>
    public const int MaxPasswordLength = 128;

    private bool _registrationEventRaised;

    /// <summary>
    ///     Parameterless constructor required by Entity Framework Core materialization.
    /// </summary>
    private User()
    {
    }

    /// <summary>
    ///     Initializes a new active user account with an unverified email and a secure verification token.
    /// </summary>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="email">The validated email address value object.</param>
    /// <param name="passwordHash">The pre-computed password hash.</param>
    /// <param name="role">The assigned platform role (defaults to <see cref="UserRole.Farmer" />).</param>
    /// <param name="phoneNumber">An optional contact phone number.</param>
    public User(
        string fullName,
        EmailAddress email,
        string passwordHash,
        UserRole role = UserRole.Farmer,
        string? phoneNumber = null) : base(UserId.Empty)
    {
        ArgumentNullException.ThrowIfNull(email);

        FullName = IamGuard.RequiredText(fullName, IamErrors.FullNameRequired, MaxFullNameLength, IamErrors.FullNameTooLong);
        Email = email;
        PasswordHash = IamGuard.RequiredText(passwordHash, IamErrors.PasswordHashRequired, MaxPasswordHashLength, IamErrors.PasswordHashRequired);
        PhoneNumber = IamGuard.OptionalText(phoneNumber, MaxPhoneNumberLength, IamErrors.PhoneNumberTooLong);
        Role = role;
        Status = UserStatus.Active;
        EmailVerified = false;
        EmailVerificationToken = GenerateVerificationToken();
    }

    /// <summary>Gets the user's full name.</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Gets the user's normalized email address.</summary>
    public EmailAddress Email { get; private set; } = null!;

    /// <summary>Gets the BCrypt password hash.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Gets the optional contact phone number.</summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>Gets the user's platform role.</summary>
    public UserRole Role { get; private set; }

    /// <summary>Gets the account lifecycle status.</summary>
    public UserStatus Status { get; private set; }

    /// <summary>Gets a value indicating whether the user's email address has been verified.</summary>
    public bool EmailVerified { get; private set; }

    /// <summary>Gets the single-use URL-safe token pending email verification, or <c>null</c> once verified.</summary>
    public string? EmailVerificationToken { get; private set; }

    /// <summary>Gets a value indicating whether the account is currently active.</summary>
    public bool IsActive => Status == UserStatus.Active;

    /// <inheritdoc />
    public DateTimeOffset? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    ///     Factory method that registers a new active <see cref="User" /> with an unverified email and
    ///     a freshly generated cryptographic verification token.
    /// </summary>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="email">The validated email address value object.</param>
    /// <param name="passwordHash">The pre-computed BCrypt password hash.</param>
    /// <param name="role">The assigned platform role (defaults to <see cref="UserRole.Farmer" />).</param>
    /// <param name="phoneNumber">An optional contact phone number.</param>
    /// <returns>A newly initialized <see cref="User" /> aggregate.</returns>
    public static User Register(
        string fullName,
        EmailAddress email,
        string passwordHash,
        UserRole role = UserRole.Farmer,
        string? phoneNumber = null)
    {
        return new User(fullName, email, passwordHash, role, phoneNumber);
    }

    /// <summary>
    ///     Updates basic profile attributes of the user.
    /// </summary>
    /// <param name="fullName">The new full name.</param>
    /// <param name="phoneNumber">The new optional phone number.</param>
    public void UpdateProfile(string fullName, string? phoneNumber)
    {
        FullName = IamGuard.RequiredText(fullName, IamErrors.FullNameRequired, MaxFullNameLength, IamErrors.FullNameTooLong);
        PhoneNumber = IamGuard.OptionalText(phoneNumber, MaxPhoneNumberLength, IamErrors.PhoneNumberTooLong);
    }

    /// <summary>
    ///     Replaces the stored password hash.
    /// </summary>
    /// <param name="passwordHash">The new BCrypt password hash.</param>
    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = IamGuard.RequiredText(
            passwordHash,
            IamErrors.PasswordHashRequired,
            MaxPasswordHashLength,
            IamErrors.PasswordHashRequired);
    }

    /// <summary>
    ///     Marks the user's email address as verified, clears the single-use verification token,
    ///     and emits <see cref="UserEmailVerifiedEvent" />.
    /// </summary>
    public void VerifyEmail()
    {
        if (EmailVerified)
        {
            EmailVerificationToken = null;
            return;
        }

        EmailVerified = true;
        EmailVerificationToken = null;

        if (Id.Value > 0)
        {
            AddDomainEvent(new UserEmailVerifiedEvent(Id.Value, Email.Value, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>Activates the user account.</summary>
    public void Activate()
    {
        Status = UserStatus.Active;
    }

    /// <summary>Deactivates the user account.</summary>
    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    /// <summary>
    ///     Raises <see cref="UserRegisteredEvent" /> once the user has been persisted and its database identity is known.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the user has not been persisted yet.</exception>
    public void MarkAsRegistered()
    {
        if (_registrationEventRaised)
        {
            return;
        }

        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The user must be persisted before raising its registration event.");
        }

        AddDomainEvent(new UserRegisteredEvent(
            Id.Value,
            FullName,
            Email.Value,
            Role.ToString(),
            DateTimeOffset.UtcNow));
        _registrationEventRaised = true;
    }

    /// <summary>
    ///     Records a successful sign-in domain event.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the user has not been persisted yet.</exception>
    public void RecordSignIn()
    {
        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The user must be persisted before recording a sign-in event.");
        }

        AddDomainEvent(new UserSignedInEvent(Id.Value, Email.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>
    ///     Records an explicit sign-out domain event.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the user has not been persisted yet.</exception>
    public void RecordSignOut()
    {
        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The user must be persisted before recording a sign-out event.");
        }

        AddDomainEvent(new UserSignedOutEvent(Id.Value, DateTimeOffset.UtcNow));
    }

    private static string GenerateVerificationToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }
}
