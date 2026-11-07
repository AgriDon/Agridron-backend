using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;

/// <summary>
///     Use cases that change fumigation areas.
/// </summary>
internal sealed class FumigationAreaCommandService(
    IFumigationAreaRepository fumigationAreaRepository,
    IParcelRepository parcelRepository,
    IUnitOfWork unitOfWork) : IFumigationAreaCommandService
{
    public async Task<Result<FumigationArea>> Handle(
        CreateFumigationAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ParcelId <= 0)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.FumigationAreaParcelInvalid);
        }

        var parcel = await parcelRepository.FindByIdAsync(command.ParcelId, cancellationToken);
        if (parcel is null || !parcel.IsActive)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.ParcelNotFound(command.ParcelId));
        }

        try
        {
            var area = new FumigationArea(
                new ParcelId(command.ParcelId),
                GeoJsonPolygon.FromGeoJson(command.Geometry));

            area.EnsureFitsWithin(parcel.AreaHectares);

            await fumigationAreaRepository.AddAsync(area, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            area.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<FumigationArea>.Success(area);
        }
        catch (DomainException exception)
        {
            return Result<FumigationArea>.Failure(exception.Error);
        }
    }

    public async Task<Result<FumigationArea>> Handle(
        UpdateFumigationAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var area = await fumigationAreaRepository.FindByIdAsync(command.Id, cancellationToken);
        if (area is null)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.FumigationAreaNotFound(command.Id));
        }

        var parcel = await parcelRepository.FindByIdAsync(command.ParcelId, cancellationToken);
        if (parcel is null || !parcel.IsActive)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.ParcelNotFound(command.ParcelId));
        }

        try
        {
            area.MoveToParcel(new ParcelId(command.ParcelId));
            area.RedefineGeometry(GeoJsonPolygon.FromGeoJson(command.Geometry));
            area.EnsureFitsWithin(parcel.AreaHectares);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<FumigationArea>.Success(area);
        }
        catch (DomainException exception)
        {
            return Result<FumigationArea>.Failure(exception.Error);
        }
    }

    public async Task<Result> Handle(
        DeleteFumigationAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var area = await fumigationAreaRepository.FindByIdAsync(command.Id, cancellationToken);
        if (area is null)
        {
            return Result.Failure(FieldManagementErrors.FumigationAreaNotFound(command.Id));
        }

        fumigationAreaRepository.Remove(area);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public Task<Result<FumigationArea>> CreateAsync(
        long parcelId,
        string geometry,
        CancellationToken cancellationToken = default) =>
        Handle(new CreateFumigationAreaCommand(parcelId, geometry), cancellationToken);

    public Task<Result<FumigationArea>> UpdateAsync(
        long id,
        long parcelId,
        string geometry,
        CancellationToken cancellationToken = default) =>
        Handle(new UpdateFumigationAreaCommand(id, parcelId, geometry), cancellationToken);

    public Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Handle(new DeleteFumigationAreaCommand(id), cancellationToken);
}
