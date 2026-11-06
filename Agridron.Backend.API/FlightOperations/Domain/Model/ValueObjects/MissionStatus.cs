namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/// <summary>
///     Lifecycle status of a fumigation mission.
/// </summary>
public enum MissionStatus
{
    /// <summary>Initial state: requested and planned.</summary>
    Planned = 1,

    /// <summary>Formally approved by agricultural supervisor, authorized to fly.</summary>
    Authorized = 2,

    /// <summary>Currently executing in the field.</summary>
    InProgress = 3,

    /// <summary>Temporarily interrupted due to weather, obstacle or technical check.</summary>
    Paused = 4,

    /// <summary>Completed and closed with flight logs.</summary>
    Completed = 5,

    /// <summary>Cancelled by user or supervisor before or during execution.</summary>
    Cancelled = 6
}
