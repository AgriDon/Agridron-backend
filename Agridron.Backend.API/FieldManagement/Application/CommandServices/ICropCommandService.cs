using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.CommandServices;

/// <summary>
///     Use cases that change crops.
/// </summary>
public interface ICropCommandService
{
    /// <summary>Registers a crop.</summary>
    Task<Result<Crop>> CreateAsync(
        string name,
        string variety,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a crop.</summary>
    Task<Result<Crop>> UpdateAsync(
        long id,
        string name,
        string variety,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a crop when it is not assigned to any parcel.</summary>
    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
