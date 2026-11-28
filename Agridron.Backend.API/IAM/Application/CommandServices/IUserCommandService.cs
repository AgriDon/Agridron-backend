using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.IAM.Application.CommandServices;

/// <summary>
///     Application service contract for IAM use cases that mutate user state or run authentication workflows.
/// </summary>
public interface IUserCommandService
{
    /// <summary>
    ///     Registers a new user account, persists it, raises <c>UserRegisteredEvent</c>, and triggers welcome email delivery (AU-001).
    /// </summary>
    /// <param name="command">The registration command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the created <see cref="User" /> or a domain error.</returns>
    Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Authenticates a user by email and password and issues a signed JWT token (AU-003).
    /// </summary>
    /// <param name="command">The sign-in command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the authenticated <see cref="User" /> and JWT token, or an error.</returns>
    Task<Result<(User User, string Token)>> Handle(SignInCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Records an authenticated user sign-out (AU-007).
    /// </summary>
    /// <param name="command">The sign-out command containing the authenticated user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result" /> indicating success or the domain error.</returns>
    Task<Result> Handle(SignOutCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates basic profile details for an existing user (AU-005).
    /// </summary>
    /// <param name="command">The profile update command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the updated <see cref="User" /> or a domain error.</returns>
    Task<Result<User>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Verifies a user's email address using a single-use verification token.
    /// </summary>
    /// <param name="command">The email verification command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the verified <see cref="User" /> or a validation error.</returns>
    Task<Result<User>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken = default);
}
