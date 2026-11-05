using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
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
    public async Task<Result<Crop>> Handle(CreateCropCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var crop = new Crop(command.Name, command.Variety);

            if (await cropRepository.ExistsWithNameAndVarietyAsync(crop.Name, crop.Variety, cancellationToken: cancellationToken))
            {
                return Result<Crop>.Failure(FieldManagementErrors.CropAlreadyExists(crop.Name, crop.Variety));
            }

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

    public async Task<Result<Crop>> Handle(UpdateCropCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var crop = await cropRepository.FindByIdAsync(command.Id, cancellationToken);
        if (crop is null)
        {
            return Result<Crop>.Failure(FieldManagementErrors.CropNotFound(command.Id));
        }

        try
        {
            crop.UpdateDetails(command.Name, command.Variety);

            if (await cropRepository.ExistsWithNameAndVarietyAsync(crop.Name, crop.Variety, crop.Id.Value, cancellationToken))
            {
                return Result<Crop>.Failure(FieldManagementErrors.CropAlreadyExists(crop.Name, crop.Variety));
            }

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Crop>.Success(crop);
        }
        catch (DomainException exception)
        {
            return Result<Crop>.Failure(exception.Error);
        }
    }

    public async Task<Result> Handle(DeleteCropCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var crop = await cropRepository.FindByIdAsync(command.Id, cancellationToken);
        if (crop is null)
        {
            return Result.Failure(FieldManagementErrors.CropNotFound(command.Id));
        }

        if (await parcelRepository.CountByCropIdAsync(crop.Id, onlyActive: true, cancellationToken) > 0)
        {
            return Result.Failure(FieldManagementErrors.CropHasParcels(command.Id));
        }

        cropRepository.Remove(crop);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public Task<Result<Crop>> CreateAsync(string name, string variety, CancellationToken cancellationToken = default) =>
        Handle(new CreateCropCommand(name, variety), cancellationToken);

    public Task<Result<Crop>> UpdateAsync(
        long id,
        string name,
        string variety,
        CancellationToken cancellationToken = default) =>
        Handle(new UpdateCropCommand(id, name, variety), cancellationToken);

    public Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Handle(new DeleteCropCommand(id), cancellationToken);
}
