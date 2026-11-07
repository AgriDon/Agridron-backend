using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FieldManagement.Domain.Repositories;

/// <summary>
///     Persistence contract for <see cref="FumigationArea" /> aggregates.
/// </summary>
public interface IFumigationAreaRepository : IBaseRepository<FumigationArea>
{
    /// <summary>
    ///     Lists the fumigation areas of a parcel.
    /// </summary>
    /// <param name="parcelId">The parcel identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fumigation areas of the parcel.</returns>
    Task<IReadOnlyList<FumigationArea>> ListByParcelIdAsync(
        ParcelId parcelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the fumigation areas of a parcel. Used to block the deletion of a parcel that still
    ///     has fumigation areas.
    /// </summary>
    /// <param name="parcelId">The parcel identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of fumigation areas.</returns>
    Task<int> CountByParcelIdAsync(ParcelId parcelId, CancellationToken cancellationToken = default);
}
