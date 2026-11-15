namespace Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;

/**
 * The incident type is free text, so these are constants instead of an enum.
 */
public static class IncidentTypes
{

    /**
     * Mechanical failure reported on a drone.
     */
    public const string EquipmentFailure = "FALLA_TECNICA";
}
