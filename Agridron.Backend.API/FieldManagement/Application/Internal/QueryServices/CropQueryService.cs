using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for crops.
/// </summary>
internal sealed class CropQueryService(ICropRepository cropRepository) : ICropQueryService
{
    /// <inheritdoc />
    public async Task<Result<Crop>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var crop = await cropRepository.FindByIdAsync(id, cancellationToken);

        return crop is null
            ? Result<Crop>.Failure(FieldManagementErrors.CropNotFound(id))
            : Result<Crop>.Success(crop);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<Crop>>> ListAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default)
    {
        var crops = (await cropRepository.ListAsync(cancellationToken))
            .OrderBy(crop => crop.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(crop => crop.Variety, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<PagedResult<Crop>>.Success(Paging.Apply(crops, pagination));
    }
}
