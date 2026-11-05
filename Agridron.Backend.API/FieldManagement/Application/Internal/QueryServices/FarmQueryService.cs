using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for farms.
/// </summary>
internal sealed class FarmQueryService(IFarmRepository farmRepository) : IFarmQueryService
{
    /// <inheritdoc />
    public async Task<Result<Farm>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var farm = await farmRepository.FindByIdAsync(id, cancellationToken);

        return farm is null
            ? Result<Farm>.Failure(FieldManagementErrors.FarmNotFound(id))
            : Result<Farm>.Success(farm);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<Farm>>> ListAsync(
        long? ownerId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Farm> farms = ownerId.HasValue
            ? await farmRepository.ListByOwnerIdAsync(ownerId.Value, cancellationToken)
            : (await farmRepository.ListAsync(cancellationToken))
            .OrderBy(farm => farm.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<PagedResult<Farm>>.Success(Paging.Apply(farms, pagination));
    }
}
