using System.Globalization;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;

/// <summary>
///     Identifies a <c>Farm</c> aggregate.
/// </summary>
/// <param name="Value">The underlying database identifier.</param>
public readonly record struct FarmId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static FarmId Empty => new(0);

    /// <summary>
    ///     Creates a validated identifier.
    /// </summary>
    /// <param name="value">The database identifier. Must be greater than zero.</param>
    /// <returns>The identifier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the value is not positive.</exception>
    public static FarmId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "A farm id must be greater than zero.");
        }

        return new FarmId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
