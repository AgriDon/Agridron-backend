using System.Globalization;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Transform;

/// <summary>
///     Maps maintenance records between the domain and the REST layer.
/// </summary>
public static class MaintenanceRecordAssembler
{
    /// <summary>External (JSON) value of a maintenance type, the same used by the frontend.</summary>
    public static string ToExternalString(this MaintenanceType type) => type switch
    {
        MaintenanceType.Corrective => "corrective",
        _ => "preventive"
    };

    /// <summary>
    ///     Parses "preventive" / "corrective" (case-insensitive). Null or empty means "not provided".
    /// </summary>
    /// <returns>False only when a value was provided and it is not a valid type.</returns>
    public static bool TryParseType(string? value, out MaintenanceType? type)
    {
        type = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Enum.TryParse<MaintenanceType>(value.Trim(), true, out var parsed) && Enum.IsDefined(parsed))
        {
            type = parsed;
            return true;
        }

        return false;
    }

    public static MaintenanceRecordResource ToResourceFrom(MaintenanceRecord record) => new(
        record.Id.Value,
        record.DroneId.Value,
        record.PerformedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        record.Type.ToExternalString(),
        record.Technician,
        record.Description);
}
