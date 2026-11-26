using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Read use cases for parcels.
/// </summary>
internal sealed class ParcelQueryService(IParcelRepository parcelRepository) : IParcelQueryService
{
    public async Task<Result<Parcel>> Handle(GetParcelByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var parcel = await parcelRepository.FindByIdAsync(query.Id, cancellationToken);

        return parcel is null || (!query.IncludeInactive && !parcel.IsActive)
            ? Result<Parcel>.Failure(FieldManagementErrors.ParcelNotFound(query.Id))
            : Result<Parcel>.Success(parcel);
    }

    public async Task<Result<PagedResult<Parcel>>> Handle(
        GetParcelsByFarmIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var onlyActive = !query.IncludeInactive;
        IReadOnlyList<Parcel> parcels = query.FarmId.HasValue
            ? await parcelRepository.ListByFarmIdAsync(new FarmId(query.FarmId.Value), onlyActive, cancellationToken)
            : (await parcelRepository.ListAsync(cancellationToken))
            .Where(p => query.IncludeInactive || p.IsActive)
            .OrderBy(parcel => parcel.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<PagedResult<Parcel>>.Success(Paging.Apply(parcels, query.Pagination));
    }

    public Task<Result<Parcel>> GetByIdAsync(
        long id,
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        Handle(new GetParcelByIdQuery(id, includeInactive), cancellationToken);

    public Task<Result<PagedResult<Parcel>>> ListAsync(
        long? farmId,
        PaginationParameters pagination,
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        Handle(new GetParcelsByFarmIdQuery(farmId, pagination, includeInactive), cancellationToken);
}
