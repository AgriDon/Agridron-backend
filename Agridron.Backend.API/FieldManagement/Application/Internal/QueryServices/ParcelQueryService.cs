using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for parcels.
/// </summary>
internal sealed class ParcelQueryService(IParcelRepository parcelRepository) : IParcelQueryService
{
    /// <inheritdoc />
    public async Task<Result<Parcel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var parcel = await parcelRepository.FindByIdAsync(id, cancellationToken);

        return parcel is null
            ? Result<Parcel>.Failure(FieldManagementErrors.ParcelNotFound(id))
            : Result<Parcel>.Success(parcel);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<Parcel>>> ListAsync(
        long? farmId,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Parcel> parcels = farmId.HasValue
            ? await parcelRepository.ListByFarmIdAsync(new FarmId(farmId.Value), cancellationToken)
            : (await parcelRepository.ListAsync(cancellationToken))
            .OrderBy(parcel => parcel.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<PagedResult<Parcel>>.Success(Paging.Apply(parcels, pagination));
    }
}
