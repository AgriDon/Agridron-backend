using System.Reflection;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Events;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.IAM.Domain.Model;

public class UserTests
{
    [Fact]
    public void Register_ShouldNormalizeEmailSetDefaultFarmerRoleAndGenerateVerificationToken()
    {
        // Arrange
        var email = EmailAddress.Create("  Alejandro.Samir@Agridron.PE  ");

        // Act
        var user = User.Register("  Alejandro Samir  ", email, "$2a$11$hashedpasswordvalue", phoneNumber: " +51 999888777 ");

        // Assert
        user.Id.Should().Be(UserId.Empty);
        user.FullName.Should().Be("Alejandro Samir");
        user.Email.Value.Should().Be("alejandro.samir@agridron.pe");
        user.Role.Should().Be(UserRole.Farmer);
        user.Status.Should().Be(UserStatus.Active);
        user.IsActive.Should().BeTrue();
        user.EmailVerified.Should().BeFalse();
        user.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();
        user.EmailVerificationToken!.Length.Should().Be(64);
        user.PhoneNumber.Should().Be("+51 999888777");
    }

    [Theory]
    [InlineData(null, UserRole.Farmer)]
    [InlineData("", UserRole.Farmer)]
    [InlineData("farmer", UserRole.Farmer)]
    [InlineData("Operator", UserRole.Operator)]
    [InlineData("ADMIN", UserRole.Admin)]
    public void UserRoleFromName_WhenValidOrBlank_ShouldResolveExpectedRole(string? roleName, UserRole expectedRole)
    {
        // Act
        var resolved = UserRole.FromName(roleName);

        // Assert
        resolved.Should().Be(expectedRole);
    }

    [Fact]
    public void UserRoleFromName_WhenInvalid_ShouldThrowDomainException()
    {
        // Act
        var act = () => UserRole.FromName("SuperHacker");

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("UserRoleInvalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("user@localhost")]
    [InlineData("user@.com")]
    public void EmailAddress_WhenInvalid_ShouldThrowDomainException(string rawEmail)
    {
        // Arrange & Act
        var act = () => EmailAddress.Create(rawEmail);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void VerifyEmail_OnPersistedUser_ShouldMarkVerifiedClearTokenAndRaiseEvent()
    {
        // Arrange
        var user = User.Register("Farmer One", EmailAddress.Create("farmer@agridron.pe"), "$2a$11$hash");
        SetUserId(user, 9);

        // Act
        user.VerifyEmail();

        // Assert
        user.EmailVerified.Should().BeTrue();
        user.EmailVerificationToken.Should().BeNull();
        var domainEvent = user.DomainEvents.Should().ContainSingle().Subject.Should().BeOfType<UserEmailVerifiedEvent>().Subject;
        domainEvent.UserId.Should().Be(9);
        domainEvent.Email.Should().Be("farmer@agridron.pe");
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateFullNameAndPhoneNumber()
    {
        // Arrange
        var user = User.Register("Original Name", EmailAddress.Create("farmer@agridron.pe"), "$2a$11$hash");

        // Act
        user.UpdateProfile("  Updated Name  ", "  +51 912345678  ");

        // Assert
        user.FullName.Should().Be("Updated Name");
        user.PhoneNumber.Should().Be("+51 912345678");
    }

    [Fact]
    public void UpdateProfile_WhenFullNameBlank_ShouldThrowDomainException()
    {
        // Arrange
        var user = User.Register("Original Name", EmailAddress.Create("farmer@agridron.pe"), "$2a$11$hash");

        // Act
        var act = () => user.UpdateProfile("   ", null);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("UserFullNameRequired");
    }

    [Fact]
    public void DeactivateAndActivate_ShouldToggleUserStatus()
    {
        // Arrange
        var user = User.Register("Operator One", EmailAddress.Create("op@agridron.pe"), "$2a$11$hash", UserRole.Operator);

        // Act
        user.Deactivate();
        var afterDeactivate = user.IsActive;
        user.Activate();

        // Assert
        afterDeactivate.Should().BeFalse();
        user.Status.Should().Be(UserStatus.Active);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void MarkAsRegistered_OnPersistedUser_ShouldRaiseSingleUserRegisteredEvent()
    {
        // Arrange
        var user = User.Register("Farmer One", EmailAddress.Create("farmer@agridron.pe"), "$2a$11$hash");
        SetUserId(user, 7);

        // Act
        user.MarkAsRegistered();
        user.MarkAsRegistered();

        // Assert
        var domainEvent = user.DomainEvents.Should().ContainSingle().Subject.Should().BeOfType<UserRegisteredEvent>().Subject;
        domainEvent.UserId.Should().Be(7);
        domainEvent.Email.Should().Be("farmer@agridron.pe");
        domainEvent.Role.Should().Be("Farmer");
    }

    [Fact]
    public void RecordSignInAndSignOut_OnPersistedUser_ShouldRaiseCorrespondingDomainEvents()
    {
        // Arrange
        var user = User.Register("Farmer One", EmailAddress.Create("farmer@agridron.pe"), "$2a$11$hash");
        SetUserId(user, 15);

        // Act
        user.RecordSignIn();
        user.RecordSignOut();

        // Assert
        user.DomainEvents.Should().HaveCount(2);
        user.DomainEvents.OfType<UserSignedInEvent>().Should().ContainSingle(e => e.UserId == 15);
        user.DomainEvents.OfType<UserSignedOutEvent>().Should().ContainSingle(e => e.UserId == 15);
    }

    private static void SetUserId(User user, long id)
    {
        var property = typeof(User).BaseType!.BaseType!.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        property.SetValue(user, new UserId(id));
    }
}
