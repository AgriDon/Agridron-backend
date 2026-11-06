using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Value object encapsulating autonomous flight navigation and spraying parameters.
/// </summary>
public sealed class FlightParameters : ValueObject
{
    public const decimal MinAltitudeMeters = 1.0m;
    public const decimal MaxAltitudeMeters = 30.0m;
    public const decimal MinSpeedMps = 1.0m;
    public const decimal MaxSpeedMps = 20.0m;
    public const decimal MinSprayRateLitersPerHectare = 1.0m;
    public const decimal MaxSprayRateLitersPerHectare = 100.0m;
    public const decimal MinOverlapPercentage = 0.0m;
    public const decimal MaxOverlapPercentage = 80.0m;

    private FlightParameters()
    {
    }

    public FlightParameters(
        decimal altitudeMeters,
        decimal speedMetersPerSecond,
        decimal sprayRateLitersPerHectare,
        decimal overlapPercentage)
    {
        if (altitudeMeters < MinAltitudeMeters || altitudeMeters > MaxAltitudeMeters)
        {
            throw new DomainException(FlightOperationsErrors.FlightAltitudeOutOfRange);
        }

        if (speedMetersPerSecond < MinSpeedMps || speedMetersPerSecond > MaxSpeedMps)
        {
            throw new DomainException(FlightOperationsErrors.FlightSpeedOutOfRange);
        }

        if (sprayRateLitersPerHectare < MinSprayRateLitersPerHectare || sprayRateLitersPerHectare > MaxSprayRateLitersPerHectare)
        {
            throw new DomainException(FlightOperationsErrors.SprayRateOutOfRange);
        }

        if (overlapPercentage < MinOverlapPercentage || overlapPercentage > MaxOverlapPercentage)
        {
            throw new DomainException(FlightOperationsErrors.OverlapPercentageOutOfRange);
        }

        AltitudeMeters = altitudeMeters;
        SpeedMetersPerSecond = speedMetersPerSecond;
        SprayRateLitersPerHectare = sprayRateLitersPerHectare;
        OverlapPercentage = overlapPercentage;
    }

    public decimal AltitudeMeters { get; private set; }
    public decimal SpeedMetersPerSecond { get; private set; }
    public decimal SprayRateLitersPerHectare { get; private set; }
    public decimal OverlapPercentage { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AltitudeMeters;
        yield return SpeedMetersPerSecond;
        yield return SprayRateLitersPerHectare;
        yield return OverlapPercentage;
    }
}
