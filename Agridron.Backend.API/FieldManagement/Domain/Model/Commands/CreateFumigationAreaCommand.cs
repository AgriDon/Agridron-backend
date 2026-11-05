namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to define a fumigation area within a parcel.
/// </summary>
public sealed record CreateFumigationAreaCommand(
    long ParcelId,
    string Geometry);
