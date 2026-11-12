namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Represents the operational status of a drone.
/// </summary>
public enum DroneStatus
{
    /// <summary>Ready for assignment to new missions.</summary>
    Available = 1,

    /// <summary>Reserved and assigned to an upcoming mission.</summary>
    Assigned = 2,

    /// <summary>Currently executing a flight/spraying mission.</summary>
    InFlight = 3,

    /// <summary>Temporarily paused during mission execution.</summary>
    Paused = 4,

    /// <summary>Under technical maintenance or repairing.</summary>
    Maintenance = 5
}
