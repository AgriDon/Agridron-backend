namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to update an existing parcel.
/// </summary>
public sealed record UpdateParcelCommand(
    long Id,
    long FarmId,
    long CropId,
    string Name,
    string Geometry,
    string? PhenologicalStage = null,
    string? ImageUrl = null);
