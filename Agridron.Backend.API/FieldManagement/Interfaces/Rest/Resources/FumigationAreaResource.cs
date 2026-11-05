namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     A fumigation area as returned by the API.
/// </summary>
/// <param name="Id">The fumigation area identifier.</param>
/// <param name="ParcelId">The parcel the area belongs to.</param>
/// <param name="AreaHectares">The area in hectares, derived from the geometry.</param>
/// <param name="Geometry">The canonical GeoJSON polygon.</param>
public sealed record FumigationAreaResource(
    long Id,
    long ParcelId,
    decimal AreaHectares,
    string Geometry);
