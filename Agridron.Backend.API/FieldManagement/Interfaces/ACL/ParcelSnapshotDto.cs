namespace Agridron.Backend.API.FieldManagement.Interfaces.ACL;

/// <summary>
///     Data transfer object representing a snapshot of parcel data exposed for external contexts.
/// </summary>
/// <param name="ParcelId">Database identifier of the parcel.</param>
/// <param name="FarmId">Database identifier of the associated farm.</param>
/// <param name="CropId">Database identifier of the associated crop.</param>
/// <param name="Name">Name of the parcel.</param>
/// <param name="CropName">Name of the crop cultivated in this parcel, if found.</param>
/// <param name="AreaHectares">Total area of the parcel in hectares.</param>
/// <param name="IsActive">Whether the parcel is active in operations.</param>
public sealed record ParcelSnapshotDto(
    long ParcelId,
    long FarmId,
    long CropId,
    string Name,
    string? CropName,
    decimal AreaHectares,
    bool IsActive);
