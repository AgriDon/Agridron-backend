using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FieldManagement.Domain.Repositories;

/// <summary>
///     Persistence contract for <see cref="Parcel" /> aggregates.
/// </summary>
public interface IParcelRepository : IBaseRepository<Parcel>
{
    /// <summary>
    ///     Lists the parcels of a farm.
    /// </summary>
    /// <param name="farmId">The farm identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parcels of the farm, ordered by name.</returns>
    Task<IReadOnlyList<Parcel>> ListByFarmIdAsync(FarmId farmId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the parcels of a farm. Used to block the deletion of a farm that still has parcels.
    /// </summary>
    /// <param name="farmId">The farm identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of parcels.</returns>
    Task<int> CountByFarmIdAsync(FarmId farmId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the parcels that use a crop. Used to block the deletion of a crop in use.
    /// </summary>
    /// <param name="cropId">The crop identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of parcels.</returns>
    Task<int> CountByCropIdAsync(CropId cropId, CancellationToken cancellationToken = default);
}
