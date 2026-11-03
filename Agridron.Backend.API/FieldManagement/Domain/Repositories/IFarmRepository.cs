using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FieldManagement.Domain.Repositories;

/// <summary>
///     Persistence contract for <see cref="Farm" /> aggregates.
/// </summary>
public interface IFarmRepository : IBaseRepository<Farm>
{
    /// <summary>
    ///     Lists the farms owned by a user.
    /// </summary>
    /// <param name="ownerId">The owner identifier, from the IAM bounded context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The farms owned by the user, ordered by name.</returns>
    Task<IReadOnlyList<Farm>> ListByOwnerIdAsync(long ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether another farm already uses the given name.
    /// </summary>
    /// <param name="name">The farm name, case insensitive.</param>
    /// <param name="excludedId">An identifier to exclude, used while updating.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> when the name is taken.</returns>
    Task<bool> ExistsWithNameAsync(string name, long? excludedId = null, CancellationToken cancellationToken = default);
}
