using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.QueryServices;

/// <summary>
///     Read use cases for crops.
/// </summary>
public interface ICropQueryService
{
    /// <summary>Finds a crop by its identifier.</summary>
    Task<Result<Crop>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Lists every crop.</summary>
    Task<Result<PagedResult<Crop>>> ListAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);
}
