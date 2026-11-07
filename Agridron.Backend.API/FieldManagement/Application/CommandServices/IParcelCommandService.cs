using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.CommandServices;

/// <summary>
///     Use cases that change parcels.
/// </summary>
public interface IParcelCommandService
{
    /// <summary>Handles creating a parcel.</summary>
    Task<Result<Parcel>> Handle(CreateParcelCommand command, CancellationToken cancellationToken = default);

    /// <summary>Handles updating a parcel.</summary>
    Task<Result<Parcel>> Handle(UpdateParcelCommand command, CancellationToken cancellationToken = default);

    /// <summary>Handles deleting or deactivating a parcel.</summary>
    Task<Result> Handle(DeleteParcelCommand command, CancellationToken cancellationToken = default);

    /// <summary>Registers a parcel with its geometry and optional phenology.</summary>
    Task<Result<Parcel>> CreateAsync(
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? phenologicalStage = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the mutable data of a parcel.</summary>
    Task<Result<Parcel>> UpdateAsync(
        long id,
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? phenologicalStage = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deactivates a parcel (soft-delete) when it has no fumigation areas.</summary>
    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
