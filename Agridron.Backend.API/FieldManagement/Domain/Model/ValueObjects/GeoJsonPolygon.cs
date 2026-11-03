using System.Text;
using System.Text.Json;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;

/// <summary>
///     A GeoJSON <c>Polygon</c> validated and normalized at construction time.
/// </summary>
/// <remarks>
///     The frontend sends the geometry as a JSON string, so this value object parses it, validates
///     every ring and stores a canonical serialization that is safe to persist and compare.
/// </remarks>
public sealed class GeoJsonPolygon : ValueObject
{
    private const double CoordinateTolerance = 1e-9;

    private GeoJsonPolygon(string geoJson, IReadOnlyList<IReadOnlyList<GeoPoint>> rings)
    {
        GeoJson = geoJson;
        Rings = rings;
    }

    /// <summary>Canonical GeoJSON representation of the polygon.</summary>
    public string GeoJson { get; }

    /// <summary>
    ///     The rings of the polygon. The first one is the exterior ring; the rest are holes.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<GeoPoint>> Rings { get; }

    /// <summary>The exterior ring of the polygon.</summary>
    public IReadOnlyList<GeoPoint> ExteriorRing => Rings[0];

    /// <summary>
    ///     Parses and validates a GeoJSON polygon.
    /// </summary>
    /// <param name="geoJson">The GeoJSON string.</param>
    /// <returns>The validated polygon.</returns>
    /// <exception cref="DomainException">When the GeoJSON is missing or not a valid polygon.</exception>
    public static GeoJsonPolygon FromGeoJson(string? geoJson)
    {
        if (string.IsNullOrWhiteSpace(geoJson))
        {
            throw new DomainException(FieldManagementErrors.PolygonGeoJsonRequired);
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(geoJson);
        }
        catch (JsonException exception)
        {
            throw new DomainException(FieldManagementErrors.PolygonGeoJsonInvalid(exception.Message));
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new DomainException(FieldManagementErrors.PolygonGeoJsonInvalid("The root must be a JSON object."));
            }

            if (!root.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
                || !string.Equals(type.GetString(), "Polygon", StringComparison.Ordinal))
            {
                throw new DomainException(FieldManagementErrors.InvalidPolygonType);
            }

            if (!root.TryGetProperty("coordinates", out var coordinates)
                || coordinates.ValueKind != JsonValueKind.Array)
            {
                throw new DomainException(FieldManagementErrors.PolygonCoordinatesMissing);
            }

            var rings = ParseRings(coordinates);
            var normalized = Serialize(rings);

            return new GeoJsonPolygon(normalized, rings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GeoJson;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return GeoJson;
    }

    private static IReadOnlyList<IReadOnlyList<GeoPoint>> ParseRings(JsonElement coordinates)
    {
        var rings = new List<IReadOnlyList<GeoPoint>>();

        foreach (var ringElement in coordinates.EnumerateArray())
        {
            if (ringElement.ValueKind != JsonValueKind.Array)
            {
                throw new DomainException(
                    FieldManagementErrors.PolygonGeoJsonInvalid("Each ring must be an array of positions."));
            }

            var ring = new List<GeoPoint>();

            foreach (var position in ringElement.EnumerateArray())
            {
                if (position.ValueKind != JsonValueKind.Array || position.GetArrayLength() < 2)
                {
                    throw new DomainException(
                        FieldManagementErrors.PolygonGeoJsonInvalid(
                            "Each position must be an array with longitude and latitude."));
                }

                if (!position[0].TryGetDouble(out var longitude) || !position[1].TryGetDouble(out var latitude))
                {
                    throw new DomainException(
                        FieldManagementErrors.PolygonGeoJsonInvalid("Coordinates must be numbers."));
                }

                if (longitude is < -180 or > 180 || latitude is < -90 or > 90)
                {
                    throw new DomainException(FieldManagementErrors.CoordinateOutOfRange);
                }

                ring.Add(new GeoPoint(longitude, latitude));
            }

            if (ring.Count < 4)
            {
                throw new DomainException(FieldManagementErrors.PolygonTooFewPoints);
            }

            if (!IsClosed(ring))
            {
                throw new DomainException(FieldManagementErrors.PolygonRingNotClosed);
            }

            rings.Add(ring);
        }

        if (rings.Count == 0)
        {
            throw new DomainException(FieldManagementErrors.PolygonCoordinatesMissing);
        }

        return rings;
    }

    private static bool IsClosed(IReadOnlyList<GeoPoint> ring)
    {
        var first = ring[0];
        var last = ring[^1];

        return Math.Abs(first.Longitude - last.Longitude) <= CoordinateTolerance
               && Math.Abs(first.Latitude - last.Latitude) <= CoordinateTolerance;
    }

    private static string Serialize(IReadOnlyList<IReadOnlyList<GeoPoint>> rings)
    {
        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "Polygon");
            writer.WriteStartArray("coordinates");

            foreach (var ring in rings)
            {
                writer.WriteStartArray();

                foreach (var point in ring)
                {
                    writer.WriteStartArray();
                    writer.WriteNumberValue(point.Longitude);
                    writer.WriteNumberValue(point.Latitude);
                    writer.WriteEndArray();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
