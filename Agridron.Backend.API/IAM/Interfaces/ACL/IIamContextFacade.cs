namespace Agridron.Backend.API.IAM.Interfaces.ACL;

/// <summary>ACL facade exposing read-only user lookups to other bounded contexts.</summary>
public interface IIamContextFacade
{
    /// <summary>Fetches a user snapshot by database identifier, or <c>null</c> if not found.</summary>
    Task<UserSnapshotDto?> FetchUserByIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Checks whether an active user exists for the given identifier.</summary>
    Task<bool> ExistsActiveUserAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Resolves a user's database identifier from an email address.</summary>
    Task<long?> FetchUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);
}
