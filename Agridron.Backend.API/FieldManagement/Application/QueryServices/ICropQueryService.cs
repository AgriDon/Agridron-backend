using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.QueryServices;

/// <summary>
///     Read use cases for crops.
/// </summary>
public interface ICropQueryService
{
    /// <summary>Handles finding a crop by its identifier.</summary>
    Task<Result<Crop>> Handle(GetCropByIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>Handles listing all crops with pagination.</summary>
    Task<Result<PagedResult<Crop>>> Handle(GetAllCropsQuery query, CancellationToken cancellationToken = default);

    /// <summary>Finds a crop by its identifier.</summary>
    Task<Result<Crop>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Lists every crop ordered by name.</summary>
    Task<Result<PagedResult<Crop>>> ListAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);
}
