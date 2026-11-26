

using System.Globalization;
namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

/// <summary>
///     Identifies a Chemical aggregate.
/// </summary>
/// <param name="Value">The underlying database identifier.</param>
public readonly record struct ChemicalId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static ChemicalId Empty => new(0);

    /// <summary>Creates a  Value Object with validated identifier
    ///if identifier is not allow, active to argument exception
    /// </summary>
    public static ChemicalId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "A chemical id must be greater than zero.");
        }

        return new ChemicalId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
