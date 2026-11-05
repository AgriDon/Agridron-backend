using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;

/// <summary>
///     Use cases that change parcels.
/// </summary>
internal sealed class ParcelCommandService(
    IParcelRepository parcelRepository,
    IFarmRepository farmRepository,
    ICropRepository cropRepository,
    IFumigationAreaRepository fumigationAreaRepository,
    IUnitOfWork unitOfWork) : IParcelCommandService
{
    /// <inheritdoc />
    public async Task<Result<Parcel>> CreateAsync(
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? imageUrl,
        CancellationToken cancellationToken = default)
    {
        var referenceError = await ValidateReferencesAsync(farmId, cropId, cancellationToken);
        if (referenceError is not null)
        {
            return Result<Parcel>.Failure(referenceError);
        }

        try
        {
            var parcel = new Parcel(
                new FarmId(farmId),
                new CropId(cropId),
                name,
                GeoJsonPolygon.FromGeoJson(geometry),
                imageUrl);

            await parcelRepository.AddAsync(parcel, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            parcel.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Parcel>.Success(parcel);
        }
        catch (DomainException exception)
        {
            return Result<Parcel>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Parcel>> UpdateAsync(
        long id,
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? imageUrl,
        CancellationToken cancellationToken = default)
    {
        var parcel = await parcelRepository.FindByIdAsync(id, cancellationToken);
        if (parcel is null)
        {
            return Result<Parcel>.Failure(FieldManagementErrors.ParcelNotFound(id));
        }

        var referenceError = await ValidateReferencesAsync(farmId, cropId, cancellationToken);
        if (referenceError is not null)
        {
            return Result<Parcel>.Failure(referenceError);
        }

        try
        {
            parcel.MoveToFarm(new FarmId(farmId));
            parcel.AssignCrop(new CropId(cropId));
            parcel.Rename(name);
            parcel.RedefineGeometry(GeoJsonPolygon.FromGeoJson(geometry));
            parcel.ChangeImage(imageUrl);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Parcel>.Success(parcel);
        }
        catch (DomainException exception)
        {
            return Result<Parcel>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var parcel = await parcelRepository.FindByIdAsync(id, cancellationToken);
        if (parcel is null)
        {
            return Result.Failure(FieldManagementErrors.ParcelNotFound(id));
        }

        if (await fumigationAreaRepository.CountByParcelIdAsync(parcel.Id, cancellationToken) > 0)
        {
            return Result.Failure(FieldManagementErrors.ParcelHasFumigationAreas(id));
        }

        parcelRepository.Remove(parcel);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Error?> ValidateReferencesAsync(
        long farmId,
        long cropId,
        CancellationToken cancellationToken)
    {
        if (farmId <= 0)
        {
            return FieldManagementErrors.ParcelFarmInvalid;
        }

        if (cropId <= 0)
        {
            return FieldManagementErrors.ParcelCropInvalid;
        }

        if (await farmRepository.FindByIdAsync(farmId, cancellationToken) is null)
        {
            return FieldManagementErrors.FarmNotFound(farmId);
        }

        if (await cropRepository.FindByIdAsync(cropId, cancellationToken) is null)
        {
            return FieldManagementErrors.CropNotFound(cropId);
        }

        return null;
    }
}
