using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.FieldManagement.Interfaces.ACL;

namespace Agridron.Backend.API.FieldManagement.Application.ACL;

/// <summary>
///     Implementation of the Field Management ACL facade.
/// </summary>
public sealed class FieldManagementContextFacade(
    IParcelQueryService parcelQueryService,
    ICropQueryService cropQueryService) : IFieldManagementContextFacade
{
    public async Task<ParcelSnapshotDto?> FetchParcelByIdAsync(
        long parcelId,
        CancellationToken cancellationToken = default)
    {
        var parcelResult = await parcelQueryService.Handle(new GetParcelByIdQuery(parcelId), cancellationToken);
        if (parcelResult.IsFailure || parcelResult.Value is null || !parcelResult.Value.IsActive)
        {
            return null;
        }

        var parcel = parcelResult.Value;
        string? cropName = null;

        var cropResult = await cropQueryService.Handle(new GetCropByIdQuery(parcel.CropId.Value), cancellationToken);
        if (cropResult.IsSuccess && cropResult.Value is not null)
        {
            cropName = cropResult.Value.Name;
        }

        return new ParcelSnapshotDto(
            parcel.Id.Value,
            parcel.FarmId.Value,
            parcel.CropId.Value,
            parcel.Name,
            cropName,
            parcel.AreaHectares,
            parcel.IsActive);
    }

    public async Task<bool> ExistsActiveParcelAsync(
        long parcelId,
        CancellationToken cancellationToken = default)
    {
        var parcelResult = await parcelQueryService.Handle(new GetParcelByIdQuery(parcelId), cancellationToken);
        return parcelResult.IsSuccess && parcelResult.Value is not null && parcelResult.Value.IsActive;
    }
}
