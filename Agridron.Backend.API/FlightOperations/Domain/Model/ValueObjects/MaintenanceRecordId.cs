



using System.Globalization;
namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;


public readonly record struct MaintenanceRecordId(long Value)
{

/// <summary>The identifier of an aggregate that has not been persisted yet.
/// </summary>
public static MaintenanceRecordId Empty => new(0);

/// <summary>Creates a validated identifier.</summary>
public static MaintenanceRecordId From(long value)
{
    if (value <= 0)
    {
        throw new ArgumentOutOfRangeException(nameof(value), "A maintenance record id must be greater than zero.");
    }

    return new MaintenanceRecordId(value);
}

/// <summary>
/// To convert MaintenanceRecordId in text
/// </summary>
/// <returns></returns>
public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

}
