using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for farms.
/// </summary>
internal sealed class FarmQueryService(IFarmRepository farmRepository) : IFarmQueryService
{
    public async Task<Result<Farm>> Handle(GetFarmByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var farm = await farmRepository.FindByIdAsync(query.Id, cancellationToken);

        return farm is null
            ? Result<Farm>.Failure(FieldManagementErrors.FarmNotFound(query.Id))
            : Result<Farm>.Success(farm);
    }

    public async Task<Result<PagedResult<Farm>>> Handle(
        GetFarmsByOwnerIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        IReadOnlyList<Farm> farms = query.OwnerId.HasValue
            ? await farmRepository.ListByOwnerIdAsync(query.OwnerId.Value, cancellationToken)
            : (await farmRepository.ListAsync(cancellationToken)).ToList();

        return Result<PagedResult<Farm>>.Success(Paging.Apply(farms, query.Pagination));
    }

    public Task<Result<Farm>> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        Handle(new GetFarmByIdQuery(id), cancellationToken);

    public Task<Result<PagedResult<Farm>>> ListAsync(
        long? ownerId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default) =>
        Handle(new GetFarmsByOwnerIdQuery(ownerId, pagination), cancellationToken);
}
