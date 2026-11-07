using System.Globalization;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Identifies a <c>Mission</c> aggregate.
/// </summary>
/// <param name="Value">The underlying database identifier.</param>
public readonly record struct MissionId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static MissionId Empty => new(0);

    /// <summary>Creates a validated identifier.</summary>
    public static MissionId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "A mission id must be greater than zero.");
        }

        return new MissionId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
