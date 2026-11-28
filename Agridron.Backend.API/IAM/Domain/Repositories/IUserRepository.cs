using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.IAM.Domain.Repositories;

/// <summary>
///     Persistence contract for <see cref="User" /> aggregates.
/// </summary>
/// <remarks>
///     Design Pattern: Repository — decouples IAM domain and application layers from EF Core persistence details.
/// </remarks>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    ///     Finds a user by normalized email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="User" /> aggregate, or <c>null</c> if none exists.</returns>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user with the given email address already exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if an account with the email already exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a user by its pending email verification token.
    /// </summary>
    /// <param name="token">The single-use URL-safe verification token.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="User" /> aggregate, or <c>null</c> if no matching token exists.</returns>
    Task<User?> FindByVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
}
