using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FieldManagement.Domain.Repositories;

/// <summary>
///     Persistence contract for <see cref="Crop" /> aggregates.
/// </summary>
public interface ICropRepository : IBaseRepository<Crop>
{
    /// <summary>
    ///     Checks whether a crop with the same name and variety already exists.
    /// </summary>
    /// <param name="name">The crop name, case insensitive.</param>
    /// <param name="variety">The crop variety, case insensitive.</param>
    /// <param name="excludedId">An identifier to exclude, used while updating.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> when the combination is taken.</returns>
    Task<bool> ExistsWithNameAndVarietyAsync(
        string name,
        string variety,
        long? excludedId = null,
        CancellationToken cancellationToken = default);
}
