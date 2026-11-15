namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

public enum MaintenanceType
{
    /// <summary>
    /// Type maintenance to avoid or prevent possible failures
    /// </summary>
    Preventive = 1,

    /// <summary>
    /// the corrective maintenance is when drone already has a failure
    /// so the only way to return status available is with
    /// this type of maintance
    /// </summary>
    Corrective = 2
}
