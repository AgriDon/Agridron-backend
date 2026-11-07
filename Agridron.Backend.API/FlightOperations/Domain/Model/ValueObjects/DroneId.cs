using System.Globalization;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Identifies a <c>Drone</c> aggregate.
/// </summary>
/// <param name="Value">The underlying database identifier.</param>
public readonly record struct DroneId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static DroneId Empty => new(0);

    /// <summary>Creates a validated identifier.</summary>
    public static DroneId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "A drone id must be greater than zero.");
        }

        return new DroneId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
