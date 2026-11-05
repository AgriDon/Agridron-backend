using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.QueryServices;

/// <summary>
///     Read use cases for farms.
/// </summary>
public interface IFarmQueryService
{
    /// <summary>Finds a farm by its identifier.</summary>
    Task<Result<Farm>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Lists farms, optionally filtered by owner.</summary>
    Task<Result<PagedResult<Farm>>> ListAsync(
        long? ownerId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);
}
