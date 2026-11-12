using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.QueryServices;

/// <summary>
///     Read use cases for farms.
/// </summary>
public interface IFarmQueryService
{
    /// <summary>Handles finding a farm by its identifier.</summary>
    Task<Result<Farm>> Handle(GetFarmByIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>Handles listing farms by owner with pagination.</summary>
    Task<Result<PagedResult<Farm>>> Handle(GetFarmsByOwnerIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>Finds a farm by its identifier.</summary>
    Task<Result<Farm>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Lists farms, optionally filtered by owner.</summary>
    Task<Result<PagedResult<Farm>>> ListAsync(
        long? ownerId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);
}
