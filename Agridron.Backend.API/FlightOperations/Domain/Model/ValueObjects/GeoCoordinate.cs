using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing a WGS84 geographic coordinate pair.
/// </summary>
public sealed class GeoCoordinate : ValueObject
{
    private GeoCoordinate()
    {
    }

    public GeoCoordinate(decimal latitude, decimal longitude)
    {
        if (latitude < -90.0m || latitude > 90.0m)
        {
            throw new DomainException(FlightOperationsErrors.LatitudeOutOfRange);
        }

        if (longitude < -180.0m || longitude > 180.0m)
        {
            throw new DomainException(FlightOperationsErrors.LongitudeOutOfRange);
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";
}
