using Agridron.Backend.API.InventoryAndResourceManagement.Application.CommandServices;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Commands;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.Internal.CommandServices;

internal sealed class NozzleCommandService(
    INozzleRepository nozzleRepository,
    IUnitOfWork unitOfWork) : INozzleCommandService
{
    /// <summary>
    /// Method to create nozzle aggregate
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Result<Nozzle>> Handle(CreateNozzleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var nozzle = new Nozzle(command.Type, command.DiameterMm, command.StockUnits, command.Status);

            await nozzleRepository.AddAsync(nozzle, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Nozzle>.Success(nozzle);
        }
        catch (DomainException ex)
        {
            return Result<Nozzle>.Failure(ex.Error);
        }
    }

    /// <summary>
    /// Method to update nozzle
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>

    public async Task<Result<Nozzle>> Handle(UpdateNozzleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var nozzle = await nozzleRepository.FindByIdAsync(command.Id, cancellationToken);
        if (nozzle is null || !nozzle.IsActive)
        {
            return Result<Nozzle>.Failure(InventoryErrors.NozzleNotFound(command.Id));
        }

        try
        {
            nozzle.UpdateDetails(command.Type, command.DiameterMm, command.StockUnits, command.Status);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Nozzle>.Success(nozzle);
        }
        catch (DomainException ex)
        {
            return Result<Nozzle>.Failure(ex.Error);
        }
    }
/// <summary>
/// Method to delete nozzle entity.
/// This method don't delete completely the entity, only change its parameter from active to deactivate
/// </summary>
/// <param name="command"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
/// <exception cref="ArgumentNullException"></exception>
    public async Task<Result> Handle(DeleteNozzleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var nozzle = await nozzleRepository.FindByIdAsync(command.Id, cancellationToken);
        if (nozzle is null || !nozzle.IsActive)
        {
            return Result.Failure(InventoryErrors.NozzleNotFound(command.Id));
        }

        nozzle.Deactivate();
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

/// <summary>
/// Method to change de status of nozzle
/// </summary>
/// <param name="command"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
/// <exception cref="ArgumentNullException"></exception>
    public async Task<Result<Nozzle>> Handle(ToggleNozzleStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var nozzle = await nozzleRepository.FindByIdAsync(command.Id, cancellationToken);
        if (nozzle is null || !nozzle.IsActive)
        {
            return Result<Nozzle>.Failure(InventoryErrors.NozzleNotFound(command.Id));
        }

        nozzle.ToggleStatus();
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result<Nozzle>.Success(nozzle);
    }
}
