using System.Globalization;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Identifies an <c>Incident</c> aggregate.
/// </summary>
/// <param name="Value">The underlying database identifier.</param>
public readonly record struct IncidentId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static IncidentId Empty => new(0);

    /// <summary>Creates a validated identifier.</summary>
    public static IncidentId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "An incident id must be greater than zero.");
        }

        return new IncidentId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
