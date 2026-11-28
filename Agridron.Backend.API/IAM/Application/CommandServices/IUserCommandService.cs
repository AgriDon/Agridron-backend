using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.IAM.Application.CommandServices;

/// <summary>Application service contract for IAM write operations and authentication workflows.</summary>
public interface IUserCommandService
{
    /// <summary>Registers a new platform user and triggers verification email delivery.</summary>
    Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken = default);

    /// <summary>Validates credentials and issues a signed JWT for an active user.</summary>
    Task<Result<(User User, string Token)>> Handle(SignInCommand command, CancellationToken cancellationToken = default);

    /// <summary>Records an explicit sign-out event for an authenticated user.</summary>
    Task<Result> Handle(SignOutCommand command, CancellationToken cancellationToken = default);

    /// <summary>Updates basic profile attributes for an existing user.</summary>
    Task<Result<User>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken = default);

    /// <summary>Marks a user's email as verified using a single-use verification token.</summary>
    Task<Result<User>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken = default);
}
