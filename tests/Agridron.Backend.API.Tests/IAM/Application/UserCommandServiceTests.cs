using System.Reflection;
using Agridron.Backend.API.IAM.Application.Internal.CommandServices;
using Agridron.Backend.API.IAM.Application.Internal.OutboundServices;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Domain.Model.Events;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.IAM.Domain.Repositories;
using Agridron.Backend.API.Shared.Domain.Model;
using Agridron.Backend.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Agridron.Backend.API.Tests.IAM.Application;

public class UserCommandServiceTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeHashingService _hashingService = new();
    private readonly FakeTokenService _tokenService = new();
    private readonly FakeBrevoEmailService _brevoEmailService = new();
    private readonly UserCommandService _service;

    public UserCommandServiceTests()
    {
        _service = new UserCommandService(
            _userRepository,
            _unitOfWork,
            _hashingService,
            _tokenService,
            _brevoEmailService,
            NullLogger<UserCommandService>.Instance);
    }

    [Fact]
    public async Task HandleSignUp_WhenValid_ShouldRegisterUserHashPasswordRaiseEventAndSendVerificationEmailAsync()
    {
        // Arrange
        var command = new SignUpCommand("Carlos Mendoza", "Carlos@Agridron.pe", "SecurePass123!", "+51 987654321");

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Value.Should().BeGreaterThan(0);
        result.Value.Email.Value.Should().Be("carlos@agridron.pe");
        result.Value.Role.Should().Be(UserRole.Farmer);
        result.Value.EmailVerified.Should().BeFalse();
        result.Value.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();
        result.Value.PasswordHash.Should().Be("hashed::SecurePass123!");
        result.Value.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UserRegisteredEvent>();
        _brevoEmailService.SentEmails.Should().ContainSingle(e =>
            e.Email == "carlos@agridron.pe"
            && e.Role == "Farmer"
            && e.Token == result.Value.EmailVerificationToken);
        _unitOfWork.CompleteCalls.Should().Be(2);
    }

    [Fact]
    public async Task HandleSignUp_WhenEmailAlreadyExists_ShouldReturnConflictErrorAsync()
    {
        // Arrange
        await _service.Handle(new SignUpCommand("User One", "duplicate@agridron.pe", "SecurePass123!", null));

        // Act
        var result = await _service.Handle(new SignUpCommand("User Two", "DUPLICATE@agridron.pe", "AnotherPass123!", null));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("EmailAlreadyExists");
        result.Error.Message.Should().Be("Ya existe una cuenta registrada con el correo electrónico 'duplicate@agridron.pe'.");
    }

    [Fact]
    public async Task HandleSignUp_WhenBrevoFails_ShouldStillCommitRegistrationAndReturnSuccessAsync()
    {
        // Arrange
        _brevoEmailService.ShouldThrow = true;
        var command = new SignUpCommand("Resilient User", "resilient@agridron.pe", "SecurePass123!", null);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await _userRepository.ExistsByEmailAsync("resilient@agridron.pe")).Should().BeTrue();
    }

    [Fact]
    public async Task HandleVerifyEmail_WhenTokenIsValid_ShouldVerifyEmailClearTokenAndRaiseEventAsync()
    {
        // Arrange
        var signUpResult = await _service.Handle(new SignUpCommand("Verify Me", "verify@agridron.pe", "StrongPass123!", null));
        var user = signUpResult.Value!;
        var token = user.EmailVerificationToken!;
        user.ClearDomainEvents();

        // Act
        var result = await _service.Handle(new VerifyEmailCommand(token));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.EmailVerified.Should().BeTrue();
        result.Value.EmailVerificationToken.Should().BeNull();
        result.Value.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UserEmailVerifiedEvent>();
    }

    [Fact]
    public async Task HandleVerifyEmail_WhenTokenIsInvalidOrAlreadyUsed_ShouldReturnValidationErrorAsync()
    {
        // Arrange
        var signUpResult = await _service.Handle(new SignUpCommand("Verify Once", "once@agridron.pe", "StrongPass123!", null));
        var token = signUpResult.Value!.EmailVerificationToken!;
        await _service.Handle(new VerifyEmailCommand(token));

        // Act
        var secondAttempt = await _service.Handle(new VerifyEmailCommand(token));
        var unknownTokenAttempt = await _service.Handle(new VerifyEmailCommand("non-existent-token"));

        // Assert
        secondAttempt.IsFailure.Should().BeTrue();
        secondAttempt.Error!.Type.Should().Be(ErrorType.Validation);
        secondAttempt.Error.Code.Should().Be("InvalidVerificationToken");
        secondAttempt.Error.Message.Should().Be("El enlace de verificación no es válido o ya expiró.");

        unknownTokenAttempt.IsFailure.Should().BeTrue();
        unknownTokenAttempt.Error!.Type.Should().Be(ErrorType.Validation);
        unknownTokenAttempt.Error.Code.Should().Be("InvalidVerificationToken");
        unknownTokenAttempt.Error.Message.Should().Be("El enlace de verificación no es válido o ya expiró.");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("weakpass1")]
    [InlineData("WeakPass1")]
    [InlineData("WEAKPASS1!")]
    [InlineData("weakpass1!")]
    [InlineData("Wk1!")]
    public async Task HandleSignUp_WhenPasswordIsWeak_ShouldReturnValidationErrorAsync(string weakPassword)
    {
        // Arrange
        var command = new SignUpCommand("Weak User", "weak@agridron.pe", weakPassword, null);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("UserPasswordTooWeak");
        result.Error.Message.Should().Be("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
    }

    [Fact]
    public async Task HandleSignIn_WhenEmailIsNotVerified_ShouldReturnForbiddenAndAllowAfterVerificationAsync()
    {
        // Arrange
        var signUpResult = await _service.Handle(new SignUpCommand("Pending User", "pending@agridron.pe", "Secret123!", null));
        var token = signUpResult.Value!.EmailVerificationToken!;

        // Act
        var unverifiedResult = await _service.Handle(new SignInCommand("pending@agridron.pe", "Secret123!"));
        await _service.Handle(new VerifyEmailCommand(token));
        var verifiedResult = await _service.Handle(new SignInCommand("pending@agridron.pe", "Secret123!"));

        // Assert
        unverifiedResult.IsFailure.Should().BeTrue();
        unverifiedResult.Error!.Type.Should().Be(ErrorType.Forbidden);
        unverifiedResult.Error.Code.Should().Be("EmailNotVerified");
        unverifiedResult.Error.Message.Should().Be("Debes verificar tu correo electrónico antes de iniciar sesión. Revisa tu bandeja de entrada.");

        verifiedResult.IsSuccess.Should().BeTrue();
        verifiedResult.Value.Token.Should().Be($"token-for-{signUpResult.Value.Id.Value}");
    }

    [Fact]
    public async Task HandleSignIn_WhenCredentialsAreValid_ShouldReturnUserAndTokenAndRaiseEventAsync()
    {
        // Arrange
        var signUpResult = await _service.Handle(new SignUpCommand("Lucia Torres", "lucia@agridron.pe", "StrongPass1#", null));
        await _service.Handle(new VerifyEmailCommand(signUpResult.Value!.EmailVerificationToken!));
        signUpResult.Value!.ClearDomainEvents();

        // Act
        var result = await _service.Handle(new SignInCommand("LUCIA@agridron.pe", "StrongPass1#"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.User.Email.Value.Should().Be("lucia@agridron.pe");
        result.Value.User.EmailVerified.Should().BeTrue();
        result.Value.Token.Should().Be($"token-for-{signUpResult.Value.Id.Value}");
        result.Value.User.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UserSignedInEvent>();
    }

    [Fact]
    public async Task HandleSignIn_WhenPasswordIsIncorrect_ShouldReturnUnauthorizedErrorAsync()
    {
        // Arrange
        await _service.Handle(new SignUpCommand("Lucia Torres", "lucia@agridron.pe", "StrongPass123!", null));

        // Act
        var result = await _service.Handle(new SignInCommand("lucia@agridron.pe", "WrongPassword!"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        result.Error.Code.Should().Be("InvalidCredentials");
        result.Error.Message.Should().Be("Correo electrónico o contraseña incorrectos.");
    }

    [Fact]
    public async Task HandleSignIn_WhenUserIsInactive_ShouldReturnForbiddenErrorAsync()
    {
        // Arrange
        var signUpResult = await _service.Handle(new SignUpCommand("Inactive User", "inactive@agridron.pe", "StrongPass123!", null));
        signUpResult.Value!.Deactivate();

        // Act
        var result = await _service.Handle(new SignInCommand("inactive@agridron.pe", "StrongPass123!"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        result.Error.Code.Should().Be("UserInactive");
        result.Error.Message.Should().Be("La cuenta de usuario se encuentra inactiva.");
    }

    [Fact]
    public async Task HandleSignOut_WhenUserExists_ShouldRaiseSignOutEventAndSucceedAsync()
    {
        // Arrange
        var signUpResult = await _service.Handle(new SignUpCommand("Ana Diaz", "ana@agridron.pe", "StrongPass123!", null));
        var user = signUpResult.Value!;
        user.ClearDomainEvents();

        // Act
        var result = await _service.Handle(new SignOutCommand(user.Id.Value));

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UserSignedOutEvent>();
    }

    [Fact]
    public async Task HandleUpdateUserProfile_WhenUserExists_ShouldUpdateAndPersistAsync()
    {
        // Arrange
        var signUpResult = await _service.Handle(new SignUpCommand("Old Name", "profile@agridron.pe", "StrongPass123!", null));
        var userId = signUpResult.Value!.Id.Value;

        // Act
        var result = await _service.Handle(new UpdateUserProfileCommand(userId, "New Name", "+51 900111222"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FullName.Should().Be("New Name");
        result.Value.PhoneNumber.Should().Be("+51 900111222");
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];
        private long _nextId = 1;

        public Task AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id.Value == 0)
            {
                var prop = typeof(User).BaseType!.BaseType!.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
                prop.SetValue(entity, new UserId(_nextId++));
            }

            _users.Add(entity);
            return Task.CompletedTask;
        }

        public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.FirstOrDefault(u => u.Id.Value == id));

        public void Update(User entity)
        {
        }

        public void Remove(User entity) => _users.Remove(entity);

        public Task<IEnumerable<User>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<User>>(_users.ToList());

        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.FirstOrDefault(u =>
                string.Equals(u.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.Any(u =>
                string.Equals(u.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task<User?> FindByVerificationTokenAsync(string token, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.FirstOrDefault(u =>
                !string.IsNullOrWhiteSpace(u.EmailVerificationToken)
                && string.Equals(u.EmailVerificationToken, token.Trim(), StringComparison.Ordinal)));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CompleteCalls { get; private set; }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            CompleteCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHashingService : IHashingService
    {
        public string HashPassword(string password) => $"hashed::{password}";

        public bool VerifyPassword(string password, string passwordHash) =>
            string.Equals(passwordHash, $"hashed::{password}", StringComparison.Ordinal);
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateToken(User user) => $"token-for-{user.Id.Value}";
    }

    private sealed class FakeBrevoEmailService : IBrevoEmailService
    {
        public bool ShouldThrow { get; set; }
        public List<(string Email, string FullName, string Role, string Token)> SentEmails { get; } = [];

        public Task SendWelcomeEmailAsync(
            string toEmail,
            string fullName,
            string role,
            string verificationToken,
            CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new HttpRequestException("Simulated Brevo outage");
            }

            SentEmails.Add((toEmail, fullName, role, verificationToken));
            return Task.CompletedTask;
        }
    }
}
