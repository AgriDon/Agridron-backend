using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;

/// <summary>
///     Use cases that change crops.
/// </summary>
internal sealed class CropCommandService(
    ICropRepository cropRepository,
    IParcelRepository parcelRepository,
    IUnitOfWork unitOfWork) : ICropCommandService
{
    /// <inheritdoc />
    public async Task<Result<Crop>> CreateAsync(
        string name,
        string variety,
        CancellationToken cancellationToken = default)
    {
        var trimmedName = (name ?? string.Empty).Trim();
        var trimmedVariety = (variety ?? string.Empty).Trim();

        if (await cropRepository.ExistsWithNameAndVarietyAsync(
                trimmedName,
                trimmedVariety,
                cancellationToken: cancellationToken))
        {
            return Result<Crop>.Failure(
                FieldManagementErrors.CropAlreadyExists(trimmedName, trimmedVariety));
        }

        try
        {
            var crop = new Crop(trimmedName, trimmedVariety);

            await cropRepository.AddAsync(crop, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            crop.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Crop>.Success(crop);
        }
        catch (DomainException exception)
        {
            return Result<Crop>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Crop>> UpdateAsync(
        long id,
        string name,
        string variety,
        CancellationToken cancellationToken = default)
    {
        var crop = await cropRepository.FindByIdAsync(id, cancellationToken);
        if (crop is null)
        {
            return Result<Crop>.Failure(FieldManagementErrors.CropNotFound(id));
        }

        var trimmedName = (name ?? string.Empty).Trim();
        var trimmedVariety = (variety ?? string.Empty).Trim();

        if (await cropRepository.ExistsWithNameAndVarietyAsync(
                trimmedName,
                trimmedVariety,
                id,
                cancellationToken))
        {
            return Result<Crop>.Failure(
                FieldManagementErrors.CropAlreadyExists(trimmedName, trimmedVariety));
        }

        try
        {
            crop.UpdateDetails(trimmedName, trimmedVariety);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Crop>.Success(crop);
        }
        catch (DomainException exception)
        {
            return Result<Crop>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var crop = await cropRepository.FindByIdAsync(id, cancellationToken);
        if (crop is null)
        {
            return Result.Failure(FieldManagementErrors.CropNotFound(id));
        }

        if (await parcelRepository.CountByCropIdAsync(crop.Id, cancellationToken) > 0)
        {
            return Result.Failure(FieldManagementErrors.CropHasParcels(id));
        }

        cropRepository.Remove(crop);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
