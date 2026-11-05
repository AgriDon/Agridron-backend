using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.CommandServices;

/// <summary>
///     Use cases that change fumigation areas.
/// </summary>
public interface IFumigationAreaCommandService
{
    /// <summary>Handles creating a fumigation area.</summary>
    Task<Result<FumigationArea>> Handle(CreateFumigationAreaCommand command, CancellationToken cancellationToken = default);

    /// <summary>Handles updating a fumigation area.</summary>
    Task<Result<FumigationArea>> Handle(UpdateFumigationAreaCommand command, CancellationToken cancellationToken = default);

    /// <summary>Handles deleting a fumigation area.</summary>
    Task<Result> Handle(DeleteFumigationAreaCommand command, CancellationToken cancellationToken = default);

    /// <summary>Defines a fumigation area inside a parcel.</summary>
    Task<Result<FumigationArea>> CreateAsync(
        long parcelId,
        string geometry,
        CancellationToken cancellationToken = default);

    /// <summary>Redefines the polygon of a fumigation area.</summary>
    Task<Result<FumigationArea>> UpdateAsync(
        long id,
        long parcelId,
        string geometry,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a fumigation area.</summary>
    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
