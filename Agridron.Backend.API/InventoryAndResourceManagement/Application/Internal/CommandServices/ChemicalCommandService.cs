using Agridron.Backend.API.InventoryAndResourceManagement.Application.CommandServices;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Commands;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.Internal.CommandServices;

internal sealed class ChemicalCommandService(
    IChemicalRepository chemicalRepository,
    IUnitOfWork unitOfWork) : IChemicalCommandService
{
    public async Task<Result<Chemical>> Handle(CreateChemicalCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var chemical = new Chemical(
                command.Name,
                command.Type,
                command.StockLiters,
                command.MinimumStockLiters,
                command.ExpirationDate);

            await chemicalRepository.AddAsync(chemical, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Chemical>.Success(chemical);
        }
        catch (DomainException ex)
        {
            return Result<Chemical>.Failure(ex.Error);
        }
    }

    public async Task<Result<Chemical>> Handle(UpdateChemicalCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var chemical = await chemicalRepository.FindByIdAsync(command.Id, cancellationToken);
        if (chemical is null || !chemical.IsActive)
        {
            return Result<Chemical>.Failure(InventoryErrors.ChemicalNotFound(command.Id));
        }

        try
        {
            chemical.UpdateDetails(
                command.Name,
                command.Type,
                command.StockLiters,
                command.MinimumStockLiters,
                command.ExpirationDate);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Chemical>.Success(chemical);
        }
        catch (DomainException ex)
        {
            return Result<Chemical>.Failure(ex.Error);
        }
    }

    public async Task<Result> Handle(DeleteChemicalCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var chemical = await chemicalRepository.FindByIdAsync(command.Id, cancellationToken);
        if (chemical is null || !chemical.IsActive)
        {
            return Result.Failure(InventoryErrors.ChemicalNotFound(command.Id));
        }

        chemical.Deactivate();
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Chemical>> Handle(RestockChemicalCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var chemical = await chemicalRepository.FindByIdAsync(command.Id, cancellationToken);
        if (chemical is null || !chemical.IsActive)
        {
            return Result<Chemical>.Failure(InventoryErrors.ChemicalNotFound(command.Id));
        }

        try
        {
            chemical.Restock(command.Liters);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Chemical>.Success(chemical);
        }
        catch (DomainException ex)
        {
            return Result<Chemical>.Failure(ex.Error);
        }
    }
}
