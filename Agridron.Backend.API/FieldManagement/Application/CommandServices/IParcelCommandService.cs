using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.CommandServices;

/// <summary>
///     Use cases that change parcels.
/// </summary>
public interface IParcelCommandService
{
    /// <summary>Registers a parcel.</summary>
    Task<Result<Parcel>> CreateAsync(
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the mutable data of a parcel.</summary>
    Task<Result<Parcel>> UpdateAsync(
        long id,
        long farmId,
        long cropId,
        string name,
        string geometry,
        string? imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a parcel when it has no fumigation areas.</summary>
    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
