using System.Globalization;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.Tests.FieldManagement;

/// <summary>
///     Builds geometry and sets identities in tests without exposing production-only constructors.
/// </summary>
internal static class TestGeometry
{
    /// <summary>
    ///     Builds a closed square polygon in the given coordinates.
    /// </summary>
    public static string SquareGeoJson(double minLongitude, double minLatitude, double maxLongitude, double maxLatitude)
    {
        var minLon = minLongitude.ToString("R", CultureInfo.InvariantCulture);
        var minLat = minLatitude.ToString("R", CultureInfo.InvariantCulture);
        var maxLon = maxLongitude.ToString("R", CultureInfo.InvariantCulture);
        var maxLat = maxLatitude.ToString("R", CultureInfo.InvariantCulture);

        return $$"""
                 {
                   "type": "Polygon",
                   "coordinates": [
                     [
                       [{{minLon}}, {{minLat}}],
                       [{{maxLon}}, {{minLat}}],
                       [{{maxLon}}, {{maxLat}}],
                       [{{minLon}}, {{maxLat}}],
                       [{{minLon}}, {{minLat}}]
                     ]
                   ]
                 }
                 """;
    }

    /// <summary>Builds a validated square polygon.</summary>
    public static GeoJsonPolygon Square(
        double minLongitude,
        double minLatitude,
        double maxLongitude,
        double maxLatitude)
    {
        return GeoJsonPolygon.FromGeoJson(
            SquareGeoJson(minLongitude, minLatitude, maxLongitude, maxLatitude));
    }

    /// <summary>
    ///     Assigns an identity to an aggregate, simulating what Entity Framework Core does after
    ///     the insert statement returns the generated key.
    /// </summary>
    public static T WithId<T>(T aggregate, object id) where T : class
    {
        var property = aggregate.GetType().GetProperty("Id")
                       ?? throw new InvalidOperationException($"{aggregate.GetType().Name} has no Id property.");

        property.SetValue(aggregate, id);
        return aggregate;
    }
}
