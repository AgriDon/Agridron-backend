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
///     Use cases that change farms.
/// </summary>
internal sealed class FarmCommandService(
    IFarmRepository farmRepository,
    IParcelRepository parcelRepository,
    IUnitOfWork unitOfWork) : IFarmCommandService
{
    public async Task<Result<Farm>> Handle(CreateFarmCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var farm = new Farm(command.Name, command.Location, command.OwnerId, command.ImageUrl);

            await farmRepository.AddAsync(farm, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            farm.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Farm>.Success(farm);
        }
        catch (DomainException exception)
        {
            return Result<Farm>.Failure(exception.Error);
        }
    }

    public async Task<Result<Farm>> Handle(UpdateFarmCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var farm = await farmRepository.FindByIdAsync(command.Id, cancellationToken);
        if (farm is null)
        {
            return Result<Farm>.Failure(FieldManagementErrors.FarmNotFound(command.Id));
        }

        try
        {
            farm.Rename(command.Name);
            farm.Relocate(command.Location);
            farm.ChangeOwner(command.OwnerId);
            farm.ChangeImage(command.ImageUrl);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Farm>.Success(farm);
        }
        catch (DomainException exception)
        {
            return Result<Farm>.Failure(exception.Error);
        }
    }

    public async Task<Result> Handle(DeleteFarmCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var farm = await farmRepository.FindByIdAsync(command.Id, cancellationToken);
        if (farm is null)
        {
            return Result.Failure(FieldManagementErrors.FarmNotFound(command.Id));
        }

        if (await parcelRepository.CountByFarmIdAsync(farm.Id, onlyActive: true, cancellationToken) > 0)
        {
            return Result.Failure(FieldManagementErrors.FarmHasParcels(command.Id));
        }

        farmRepository.Remove(farm);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public Task<Result<Farm>> CreateAsync(
        string name,
        string location,
        long ownerId,
        string? imageUrl,
        CancellationToken cancellationToken = default) =>
        Handle(new CreateFarmCommand(name, location, ownerId, imageUrl), cancellationToken);

    public Task<Result<Farm>> UpdateAsync(
        long id,
        string name,
        string location,
        long ownerId,
        string? imageUrl,
        CancellationToken cancellationToken = default) =>
        Handle(new UpdateFarmCommand(id, name, location, ownerId, imageUrl), cancellationToken);

    public Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Handle(new DeleteFarmCommand(id), cancellationToken);
}
