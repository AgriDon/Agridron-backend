using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
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
    public async Task<Result<Parcel>> Handle(CreateParcelCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var referenceError = await ValidateReferencesAsync(command.FarmId, command.CropId, cancellationToken);
        if (referenceError is not null)
        {
            return Result<Parcel>.Failure(referenceError);
        }

        try
        {
            var parcel = new Parcel(
                new FarmId(command.FarmId),
                new CropId(command.CropId),
                command.Name,
                GeoJsonPolygon.FromGeoJson(command.Geometry),
                command.PhenologicalStage,
                command.ImageUrl);

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

    public async Task<Result<Parcel>> Handle(UpdateParcelCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await parcelRepository.FindByIdAsync(command.Id, cancellationToken);
        if (parcel is null || !parcel.IsActive)
        {
            return Result<Parcel>.Failure(FieldManagementErrors.ParcelNotFound(command.Id));
        }

        var referenceError = await ValidateReferencesAsync(command.FarmId, command.CropId, cancellationToken);
        if (referenceError is not null)
        {
            return Result<Parcel>.Failure(referenceError);
        }

        try
        {
            parcel.MoveToFarm(new FarmId(command.FarmId));
            parcel.AssignCrop(new CropId(command.CropId));
            parcel.Rename(command.Name);
            parcel.RedefineGeometry(GeoJsonPolygon.FromGeoJson(command.Geometry));
            parcel.UpdatePhenologicalStage(command.PhenologicalStage);
            parcel.ChangeImage(command.ImageUrl);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Parcel>.Success(parcel);
        }
        catch (DomainException exception)
        {
            return Result<Parcel>.Failure(exception.Error);
        }
    }

    public async Task<Result> Handle(DeleteParcelCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var parcel = await parcelRepository.FindByIdAsync(command.Id, cancellationToken);
        if (parcel is null || !parcel.IsActive)
        {
            return Result.Failure(FieldManagementErrors.ParcelNotFound(command.Id));
        }

        if (await fumigationAreaRepository.CountByParcelIdAsync(parcel.Id, cancellationToken) > 0)
        {
            return Result.Failure(FieldManagementErrors.ParcelHasFumigationAreas(command.Id));
        }

        // Soft-delete: inactivate parcel to preserve operational and flight mission history (FM-006)
        parcel.Deactivate();
        parcelRepository.Update(parcel);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public Task<Result<Parcel>> CreateAsync(
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? phenologicalStage = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default) =>
        Handle(new CreateParcelCommand(farmId, cropId, name, geometry, phenologicalStage, imageUrl), cancellationToken);

    public Task<Result<Parcel>> UpdateAsync(
        long id,
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? phenologicalStage = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default) =>
        Handle(new UpdateParcelCommand(id, farmId, cropId, name, geometry, phenologicalStage, imageUrl), cancellationToken);

    public Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Handle(new DeleteParcelCommand(id), cancellationToken);

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
