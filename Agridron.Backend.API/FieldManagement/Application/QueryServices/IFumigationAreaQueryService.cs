using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.QueryServices;

/// <summary>
///     Read use cases for fumigation areas.
/// </summary>
public interface IFumigationAreaQueryService
{
    /// <summary>Finds a fumigation area by its identifier.</summary>
    Task<Result<FumigationArea>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Lists fumigation areas, optionally filtered by parcel.</summary>
    Task<Result<PagedResult<FumigationArea>>> ListAsync(
        long? parcelId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);
}
