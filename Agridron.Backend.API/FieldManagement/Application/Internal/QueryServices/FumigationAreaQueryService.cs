using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for fumigation areas.
/// </summary>
internal sealed class FumigationAreaQueryService(
    IFumigationAreaRepository fumigationAreaRepository) : IFumigationAreaQueryService
{
    public async Task<Result<FumigationArea>> Handle(
        GetFumigationAreaByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var area = await fumigationAreaRepository.FindByIdAsync(query.Id, cancellationToken);

        return area is null
            ? Result<FumigationArea>.Failure(FieldManagementErrors.FumigationAreaNotFound(query.Id))
            : Result<FumigationArea>.Success(area);
    }

    public async Task<Result<PagedResult<FumigationArea>>> Handle(
        GetFumigationAreasByParcelIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IReadOnlyList<FumigationArea> areas = query.ParcelId.HasValue
            ? await fumigationAreaRepository.ListByParcelIdAsync(
                new ParcelId(query.ParcelId.Value),
                cancellationToken)
            : (await fumigationAreaRepository.ListAsync(cancellationToken)).ToList();

        return Result<PagedResult<FumigationArea>>.Success(Paging.Apply(areas, query.Pagination));
    }

    public Task<Result<FumigationArea>> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        Handle(new GetFumigationAreaByIdQuery(id), cancellationToken);

    public Task<Result<PagedResult<FumigationArea>>> ListAsync(
        long? parcelId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default) =>
        Handle(new GetFumigationAreasByParcelIdQuery(parcelId, pagination), cancellationToken);
}
