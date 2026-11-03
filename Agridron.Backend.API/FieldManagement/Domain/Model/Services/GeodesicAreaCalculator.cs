using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Services;

/// <summary>
///     Computes the area of a GeoJSON polygon on the WGS 84 ellipsoid approximated by a sphere.
/// </summary>
/// <remarks>
///     Uses the spherical excess formula, which is accurate enough for agricultural parcels
///     (error well below 1%). The result is returned in hectares because that is the unit used
///     by the product and by the database column.
/// </remarks>
public static class GeodesicAreaCalculator
{
    /// <summary>Mean Earth radius in meters.</summary>
    public const double EarthRadiusMeters = 6_371_008.8;

    private const double SquareMetersPerHectare = 10_000d;

    /// <summary>
    ///     Calculates the area of the polygon in square meters, subtracting the holes.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area in square meters.</returns>
    public static double CalculateSquareMeters(GeoJsonPolygon polygon)
    {
        ArgumentNullException.ThrowIfNull(polygon);

        var exterior = RingArea(polygon.Rings[0]);
        var holes = 0d;

        for (var i = 1; i < polygon.Rings.Count; i++)
        {
            holes += RingArea(polygon.Rings[i]);
        }

        return Math.Max(0d, exterior - holes);
    }

    /// <summary>
    ///     Calculates the area of the polygon in hectares, rounded to two decimals.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area in hectares.</returns>
    public static decimal CalculateHectares(GeoJsonPolygon polygon)
    {
        var hectares = CalculateSquareMeters(polygon) / SquareMetersPerHectare;
        return Math.Round((decimal)hectares, 2, MidpointRounding.AwayFromZero);
    }

    private static double RingArea(IReadOnlyList<GeoPoint> ring)
    {
        var count = ring.Count;
        if (count < 4)
        {
            return 0d;
        }

        var sum = 0d;

        for (var i = 0; i < count; i++)
        {
            var previous = ring[(i + count - 1) % count];
            var current = ring[i];
            var next = ring[(i + 1) % count];

            sum += (ToRadians(next.Longitude) - ToRadians(previous.Longitude))
                   * Math.Sin(ToRadians(current.Latitude));
        }

        return Math.Abs(sum * EarthRadiusMeters * EarthRadiusMeters / 2d);
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }
}
