namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to redefine a fumigation area within a parcel.
/// </summary>
public sealed record UpdateFumigationAreaCommand(
    long Id,
    long ParcelId,
    string Geometry);
