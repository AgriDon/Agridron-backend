namespace Agridron.Backend.API.FieldManagement.Interfaces.ACL;

/// <summary>
///     Anti-Corruption Layer facade exposing Field Management operations to other bounded contexts.
/// </summary>
public interface IFieldManagementContextFacade
{
    /// <summary>
    ///     Fetches a snapshot of a parcel by its identifier.
    /// </summary>
    /// <param name="parcelId">Database identifier of the parcel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The snapshot if found and active; otherwise null.</returns>
    Task<ParcelSnapshotDto?> FetchParcelByIdAsync(long parcelId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether an active parcel exists by its identifier.
    /// </summary>
    /// <param name="parcelId">Database identifier of the parcel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if an active parcel exists; otherwise false.</returns>
    Task<bool> ExistsActiveParcelAsync(long parcelId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether an active farm exists by its identifier.
    /// </summary>
    /// <param name="farmId">Database identifier of the farm.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if an active farm exists; otherwise false.</returns>
    Task<bool> ExistsActiveFarmAsync(long farmId, CancellationToken cancellationToken = default);
}
