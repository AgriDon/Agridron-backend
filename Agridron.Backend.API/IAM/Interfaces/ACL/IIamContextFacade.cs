namespace Agridron.Backend.API.IAM.Interfaces.ACL;

/// <summary>
///     Anti-Corruption Layer facade exposing read-only IAM user lookups to other bounded contexts.
/// </summary>
/// <remarks>
///     Design Pattern: Facade &amp; Anti-Corruption Layer (ACL) — provides a minimal, stable cross-context
///     contract so <c>FieldManagement</c> and <c>FlightOperations</c> can verify users without coupling to IAM internals.
/// </remarks>
public interface IIamContextFacade
{
    /// <summary>
    ///     Fetches a read-only user snapshot by its database identifier.
    /// </summary>
    /// <param name="userId">The database identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="UserSnapshotDto" /> if found; otherwise <c>null</c>.</returns>
    Task<UserSnapshotDto?> FetchUserByIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether an active user exists with the given database identifier.
    /// </summary>
    /// <param name="userId">The database identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the user exists and is active; otherwise <c>false</c>.</returns>
    Task<bool> ExistsActiveUserAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fetches the database identifier of a user by email address.
    /// </summary>
    /// <param name="email">The email address to resolve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user identifier if found; otherwise <c>null</c>.</returns>
    Task<long?> FetchUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);
}
