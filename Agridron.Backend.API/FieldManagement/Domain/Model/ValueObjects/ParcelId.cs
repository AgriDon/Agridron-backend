using System.Globalization;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;

/// <summary>
///     Identifies a <c>Parcel</c> aggregate.
/// </summary>
/// <param name="Value">The underlying database identifier.</param>
public readonly record struct ParcelId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static ParcelId Empty => new(0);

    /// <summary>
    ///     Creates a validated identifier.
    /// </summary>
    /// <param name="value">The database identifier. Must be greater than zero.</param>
    /// <returns>The identifier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the value is not positive.</exception>
    public static ParcelId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "A parcel id must be greater than zero.");
        }

        return new ParcelId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
