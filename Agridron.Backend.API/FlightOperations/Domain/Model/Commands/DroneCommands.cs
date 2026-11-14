namespace Agridron.Backend.API.FlightOperations.Domain.Model.Commands;

public sealed record CreateDroneCommand(
    string SerialNumber,
    string Model,
    decimal CapacityLiters,
    long? NozzleId = null,
    string? ImageUrl = null);

public sealed record UpdateDroneCommand(
    long Id,
    string Model,
    decimal CapacityLiters,
    long? NozzleId = null,
    string? ImageUrl = null,
    DateTimeOffset? LastMaintenanceDate = null,
    DateTimeOffset? NextMaintenanceDate = null);

public sealed record DeleteDroneCommand(long Id);

/**
 * Takes an available drone out of operation for a planned maintenance
 */
public sealed record SendDroneToMaintenanceCommand(long Id);

/**
 * Registers a finished maintenance or repair and returns the drone to service
 */
public sealed record CompleteDroneMaintenanceCommand(
    long Id,
    DateTimeOffset MaintenanceDate,
    DateTimeOffset? NextMaintenanceDate = null);


/**
 * Reports a mechanical failure: the drone goes out of service and an incident is recorded
 */
public sealed record ReportDroneFailureCommand(
    long Id,
    string Description,
    DateTimeOffset? OccurredAt = null
            );
