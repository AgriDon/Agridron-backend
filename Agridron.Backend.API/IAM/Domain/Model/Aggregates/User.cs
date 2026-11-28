using System.Security.Cryptography;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.IAM.Domain.Model.Events;
using Agridron.Backend.API.IAM.Domain.Model.Validation;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;

namespace Agridron.Backend.API.IAM.Domain.Model.Aggregates;

/// <summary>Platform user aggregate root.</summary>
public sealed class User : AggregateRoot<UserId>, IAuditableEntity
{
    public const int MaxFullNameLength = 150;
    public const int MaxEmailLength = EmailAddress.MaxLength;
    public const int MaxPasswordHashLength = 255;
    public const int MaxPhoneNumberLength = 30;
    public const int MaxVerificationTokenLength = 128;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    private bool _registrationEventRaised;

    private User()
    {
    }

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

    public string FullName { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public bool EmailVerified { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public bool IsActive => Status == UserStatus.Active;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Creates a new active user with an unverified email and a single-use verification token.</summary>
    public static User Register(
        string fullName,
        EmailAddress email,
        string passwordHash,
        UserRole role = UserRole.Farmer,
        string? phoneNumber = null)
    {
        return new User(fullName, email, passwordHash, role, phoneNumber);
    }

    /// <summary>Updates the user's full name and optional phone number.</summary>
    public void UpdateProfile(string fullName, string? phoneNumber)
    {
        FullName = IamGuard.RequiredText(fullName, IamErrors.FullNameRequired, MaxFullNameLength, IamErrors.FullNameTooLong);
        PhoneNumber = IamGuard.OptionalText(phoneNumber, MaxPhoneNumberLength, IamErrors.PhoneNumberTooLong);
    }

    /// <summary>Replaces the stored password hash.</summary>
    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = IamGuard.RequiredText(
            passwordHash,
            IamErrors.PasswordHashRequired,
            MaxPasswordHashLength,
            IamErrors.PasswordHashRequired);
    }

    /// <summary>Marks the email address as verified, clears the verification token, and queues <see cref="UserEmailVerifiedEvent" />.</summary>
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

    /// <summary>Activates the account.</summary>
    public void Activate()
    {
        Status = UserStatus.Active;
    }

    /// <summary>Deactivates the account.</summary>
    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    /// <summary>Queues <see cref="UserRegisteredEvent" /> once the aggregate has a persisted identifier.</summary>
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

    /// <summary>Queues <see cref="UserSignedInEvent" /> for a persisted user.</summary>
    public void RecordSignIn()
    {
        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The user must be persisted before recording a sign-in event.");
        }

        AddDomainEvent(new UserSignedInEvent(Id.Value, Email.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Queues <see cref="UserSignedOutEvent" /> for a persisted user.</summary>
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
