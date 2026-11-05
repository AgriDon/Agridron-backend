using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
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
    IFumigationAreaRepository repository,
    IParcelRepository parcelRepository,
    IUnitOfWork unitOfWork) : IFumigationAreaCommandService
{
    /// <inheritdoc />
    public async Task<Result<FumigationArea>> CreateAsync(
        long parcelId,
        string geometry,
        CancellationToken cancellationToken = default)
    {
        if (parcelId <= 0)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.FumigationAreaParcelInvalid);
        }

        var parcel = await parcelRepository.FindByIdAsync(parcelId, cancellationToken);
        if (parcel is null)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.ParcelNotFound(parcelId));
        }

        try
        {
            var area = new FumigationArea(
                new ParcelId(parcelId),
                GeoJsonPolygon.FromGeoJson(geometry));

            area.EnsureFitsWithin(parcel.AreaHectares);

            await repository.AddAsync(area, cancellationToken);
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

    /// <inheritdoc />
    public async Task<Result<FumigationArea>> UpdateAsync(
        long id,
        long parcelId,
        string geometry,
        CancellationToken cancellationToken = default)
    {
        var area = await repository.FindByIdAsync(id, cancellationToken);
        if (area is null)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.FumigationAreaNotFound(id));
        }

        if (parcelId <= 0)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.FumigationAreaParcelInvalid);
        }

        var parcel = await parcelRepository.FindByIdAsync(parcelId, cancellationToken);
        if (parcel is null)
        {
            return Result<FumigationArea>.Failure(FieldManagementErrors.ParcelNotFound(parcelId));
        }

        try
        {
            area.MoveToParcel(new ParcelId(parcelId));
            area.RedefineGeometry(GeoJsonPolygon.FromGeoJson(geometry));
            area.EnsureFitsWithin(parcel.AreaHectares);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<FumigationArea>.Success(area);
        }
        catch (DomainException exception)
        {
            return Result<FumigationArea>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var area = await repository.FindByIdAsync(id, cancellationToken);
        if (area is null)
        {
            return Result.Failure(FieldManagementErrors.FumigationAreaNotFound(id));
        }

        repository.Remove(area);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
