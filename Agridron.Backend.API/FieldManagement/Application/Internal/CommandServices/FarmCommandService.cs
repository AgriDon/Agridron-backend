using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
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
    /// <inheritdoc />
    public async Task<Result<Farm>> CreateAsync(
        string name,
        string location,
        long ownerId,
        string? imageUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var farm = new Farm(name, location, ownerId, imageUrl);

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

    /// <inheritdoc />
    public async Task<Result<Farm>> UpdateAsync(
        long id,
        string name,
        string location,
        long ownerId,
        string? imageUrl,
        CancellationToken cancellationToken = default)
    {
        var farm = await farmRepository.FindByIdAsync(id, cancellationToken);
        if (farm is null)
        {
            return Result<Farm>.Failure(FieldManagementErrors.FarmNotFound(id));
        }

        try
        {
            farm.Rename(name);
            farm.Relocate(location);
            farm.ChangeOwner(ownerId);
            farm.ChangeImage(imageUrl);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Farm>.Success(farm);
        }
        catch (DomainException exception)
        {
            return Result<Farm>.Failure(exception.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var farm = await farmRepository.FindByIdAsync(id, cancellationToken);
        if (farm is null)
        {
            return Result.Failure(FieldManagementErrors.FarmNotFound(id));
        }

        if (await parcelRepository.CountByFarmIdAsync(farm.Id, cancellationToken) > 0)
        {
            return Result.Failure(FieldManagementErrors.FarmHasParcels(id));
        }

        farmRepository.Remove(farm);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
