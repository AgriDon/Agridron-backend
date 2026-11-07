using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.CommandServices;

/// <summary>
///     Use cases that change farms.
/// </summary>
public interface IFarmCommandService
{
    /// <summary>Handles creating a farm.</summary>
    Task<Result<Farm>> Handle(CreateFarmCommand command, CancellationToken cancellationToken = default);

    /// <summary>Handles updating a farm.</summary>
    Task<Result<Farm>> Handle(UpdateFarmCommand command, CancellationToken cancellationToken = default);

    /// <summary>Handles deleting a farm.</summary>
    Task<Result> Handle(DeleteFarmCommand command, CancellationToken cancellationToken = default);

    /// <summary>Registers a farm.</summary>
    Task<Result<Farm>> CreateAsync(
        string name,
        string location,
        long ownerId,
        string? imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the mutable data of a farm.</summary>
    Task<Result<Farm>> UpdateAsync(
        long id,
        string name,
        string location,
        long ownerId,
        string? imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a farm when it has no parcels.</summary>
    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
