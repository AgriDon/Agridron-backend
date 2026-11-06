using System.Globalization;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Identifies an <c>OperationStatus</c> aggregate.
/// </summary>
/// <param name="Value">The underlying database identifier.</param>
public readonly record struct OperationStatusId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static OperationStatusId Empty => new(0);

    /// <summary>Creates a validated identifier.</summary>
    public static OperationStatusId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "An operation status id must be greater than zero.");
        }

        return new OperationStatusId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
