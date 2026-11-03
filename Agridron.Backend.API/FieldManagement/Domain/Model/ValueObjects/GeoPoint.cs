using System.Globalization;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;

/// <summary>
///     A geographic position expressed in WGS 84.
/// </summary>
/// <param name="Longitude">Longitude in decimal degrees, between -180 and 180.</param>
/// <param name="Latitude">Latitude in decimal degrees, between -90 and 90.</param>
public readonly record struct GeoPoint(double Longitude, double Latitude)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"({Longitude}, {Latitude})");
    }
}
