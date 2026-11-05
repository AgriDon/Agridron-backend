using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for fumigation areas.
/// </summary>
internal sealed class FumigationAreaQueryService(IFumigationAreaRepository repository)
    : IFumigationAreaQueryService
{
    /// <inheritdoc />
    public async Task<Result<FumigationArea>> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var area = await repository.FindByIdAsync(id, cancellationToken);

        return area is null
            ? Result<FumigationArea>.Failure(FieldManagementErrors.FumigationAreaNotFound(id))
            : Result<FumigationArea>.Success(area);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<FumigationArea>>> ListAsync(
        long? parcelId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FumigationArea> areas = parcelId.HasValue
            ? await repository.ListByParcelIdAsync(new ParcelId(parcelId.Value), cancellationToken)
            : (await repository.ListAsync(cancellationToken))
            .OrderBy(area => area.Id.Value)
            .ToList();

        return Result<PagedResult<FumigationArea>>.Success(Paging.Apply(areas, pagination));
    }
}
