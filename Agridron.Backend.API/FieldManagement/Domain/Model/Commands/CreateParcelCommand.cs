namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to register a new parcel.
/// </summary>
public sealed record CreateParcelCommand(
    long FarmId,
    long CropId,
    string Name,
    string Geometry,
    string? PhenologicalStage = null,
    string? ImageUrl = null);
