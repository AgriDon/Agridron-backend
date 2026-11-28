using Agridron.Backend.API.IAM.Application.CommandServices;
using Agridron.Backend.API.IAM.Application.Internal.OutboundServices;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.IAM.Domain.Model.Validation;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.IAM.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Agridron.Backend.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Application command service orchestrating user registration, email verification, sign-in, sign-out, and profile updates.
/// </summary>
/// <remarks>
///     Design Pattern: Coordinates Repository, Unit of Work, Strategy (<see cref="IHashingService" />,
///     <see cref="ITokenService" />, <see cref="IBrevoEmailService" />), and Factory Method (<see cref="User.Register" />,
///     <see cref="UserRoleExtensions" />, <see cref="EmailAddress.Create" />) patterns.
/// </remarks>
internal sealed class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IHashingService hashingService,
    ITokenService tokenService,
    IBrevoEmailService brevoEmailService,
    ILogger<UserCommandService> logger) : IUserCommandService
{
    /// <inheritdoc />
    public async Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var email = EmailAddress.Create(command.Email);
            IamGuard.ValidateRawPassword(command.Password, User.MinPasswordLength, User.MaxPasswordLength);
            var role = UserRole.FromName(command.Role);

            if (await userRepository.ExistsByEmailAsync(email.Value, cancellationToken))
            {
                return Result<User>.Failure(IamErrors.EmailAlreadyExists(email.Value));
            }

            var passwordHash = hashingService.HashPassword(command.Password);
            var user = User.Register(command.FullName, email, passwordHash, role, command.PhoneNumber);

            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            user.MarkAsRegistered();
            await unitOfWork.CompleteAsync(cancellationToken);

            logger.LogInformation("User {UserId} registered with role {Role}", user.Id.Value, user.Role);

            await TrySendWelcomeEmailAsync(user, cancellationToken);

            return Result<User>.Success(user);
        }
        catch (DomainException exception)
        {
            return Result<User>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result<(User User, string Token)>> Handle(
        SignInCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return Result<(User User, string Token)>.Failure(IamErrors.InvalidCredentials);
        }

        EmailAddress email;
        try
        {
            email = EmailAddress.Create(command.Email);
        }
        catch (DomainException)
        {
            return Result<(User User, string Token)>.Failure(IamErrors.InvalidCredentials);
        }

        var user = await userRepository.FindByEmailAsync(email.Value, cancellationToken);
        if (user is null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
        {
            return Result<(User User, string Token)>.Failure(IamErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result<(User User, string Token)>.Failure(IamErrors.UserInactive);
        }

        try
        {
            var token = tokenService.GenerateToken(user);

            user.RecordSignIn();
            await unitOfWork.CompleteAsync(cancellationToken);

            logger.LogInformation("User {UserId} signed in successfully", user.Id.Value);

            return Result<(User User, string Token)>.Success((user, token));
        }
        catch (DomainException exception)
        {
            return Result<(User User, string Token)>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result> Handle(SignOutCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.UserId <= 0)
        {
            return Result.Failure(IamErrors.UserIdInvalid);
        }

        var user = await userRepository.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(IamErrors.UserNotFound(command.UserId));
        }

        user.RecordSignOut();
        await unitOfWork.CompleteAsync(cancellationToken);

        logger.LogInformation("User {UserId} signed out", user.Id.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<User>> Handle(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var user = await userRepository.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<User>.Failure(IamErrors.UserNotFound(command.UserId));
        }

        try
        {
            user.UpdateProfile(command.FullName, command.PhoneNumber);
            await unitOfWork.CompleteAsync(cancellationToken);

            logger.LogInformation("User {UserId} profile updated", user.Id.Value);

            return Result<User>.Success(user);
        }
        catch (DomainException exception)
        {
            return Result<User>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result<User>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return Result<User>.Failure(IamErrors.InvalidVerificationToken);
        }

        var user = await userRepository.FindByVerificationTokenAsync(command.Token.Trim(), cancellationToken);
        if (user is null)
        {
            return Result<User>.Failure(IamErrors.InvalidVerificationToken);
        }

        user.VerifyEmail();
        await unitOfWork.CompleteAsync(cancellationToken);

        logger.LogInformation("User {UserId} verified email address", user.Id.Value);

        return Result<User>.Success(user);
    }

    private async Task TrySendWelcomeEmailAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            await brevoEmailService.SendWelcomeEmailAsync(
                user.Email.Value,
                user.FullName,
                user.Role.ToString(),
                user.EmailVerificationToken ?? string.Empty,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to send welcome email for user {UserId}. Registration was kept committed.",
                user.Id.Value);
        }
    }
}
