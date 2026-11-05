using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.QueryServices;

/// <summary>
///     Read use cases for parcels.
/// </summary>
public interface IParcelQueryService
{
    /// <summary>Finds a parcel by its identifier.</summary>
    Task<Result<Parcel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Lists parcels, optionally filtered by farm.</summary>
    Task<Result<PagedResult<Parcel>>> ListAsync(
        long? farmId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);
}
