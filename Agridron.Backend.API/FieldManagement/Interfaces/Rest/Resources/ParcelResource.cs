namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     A parcel as returned by the API.
/// </summary>
/// <param name="Id">The parcel identifier.</param>
/// <param name="FarmId">The farm the parcel belongs to.</param>
/// <param name="CropId">The crop grown in the parcel.</param>
/// <param name="Name">The parcel name.</param>
/// <param name="AreaHectares">The area in hectares, derived from the geometry.</param>
/// <param name="Geometry">The canonical GeoJSON polygon.</param>
/// <param name="ImageUrl">An optional image URL.</param>
public sealed record ParcelResource(
    long Id,
    long FarmId,
    long CropId,
    string Name,
    decimal AreaHectares,
    string Geometry,
    string? ImageUrl);
