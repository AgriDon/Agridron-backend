using System.Globalization;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;


/// <summary>
///     Identifies a Nozzle aggregate
/// </summary>
/// <param name="Value"></param>
public readonly record struct NozzleId(long Value)
{
    /// <summary>The identifier of an aggregate that has not been persisted yet.</summary>
    public static NozzleId Empty => new(0);

    /// <summary>Creates a validated identifier
    /// if identifier's not validated so activated an argument exception
    /// .</summary>
    public static NozzleId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "A nozzle id must be greater than zero.");
        }

        return new NozzleId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
