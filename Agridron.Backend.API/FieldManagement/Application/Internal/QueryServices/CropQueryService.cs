using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for crops.
/// </summary>
internal sealed class CropQueryService(ICropRepository cropRepository) : ICropQueryService
{
    public async Task<Result<Crop>> Handle(GetCropByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var crop = await cropRepository.FindByIdAsync(query.Id, cancellationToken);

        return crop is null
            ? Result<Crop>.Failure(FieldManagementErrors.CropNotFound(query.Id))
            : Result<Crop>.Success(crop);
    }

    public async Task<Result<PagedResult<Crop>>> Handle(
        GetAllCropsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var crops = (await cropRepository.ListAsync(cancellationToken))
            .OrderBy(crop => crop.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<PagedResult<Crop>>.Success(Paging.Apply(crops, query.Pagination));
    }

    public Task<Result<Crop>> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        Handle(new GetCropByIdQuery(id), cancellationToken);

    public Task<Result<PagedResult<Crop>>> ListAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default) =>
        Handle(new GetAllCropsQuery(pagination), cancellationToken);
}
