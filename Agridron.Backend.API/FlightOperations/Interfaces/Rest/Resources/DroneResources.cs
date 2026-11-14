using System.Text.Json.Serialization;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

public sealed record DroneResource(
    long Id,
    string SerialNumber,
    string Model,
    decimal Capacity,
    decimal CapacityLiters,
    string Status,
    decimal Battery,
    decimal BatteryLevel,
    decimal FlightHours,
    long? NozzleId,
    DateTimeOffset? LastMaintenanceDate,
    DateTimeOffset? NextMaintenanceDate,
    string? ImageUrl,
    string? Urlimg,
    bool IsActive,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateDroneResource(
    string SerialNumber,
    string Model,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? Capacity,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? CapacityLiters,
    long? NozzleId,
    string? Urlimg,
    string? ImageUrl);

public sealed record UpdateDroneResource(
    string Model,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? Capacity,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? CapacityLiters,
    long? NozzleId,
    string? Urlimg,
    string? ImageUrl,
    DateTimeOffset? LastMaintenanceDate,
    DateTimeOffset? NextMaintenanceDate);


/**
 * Body of POST  /api/v1/drones/{id}/maintenance/complete
 */
public sealed record CompleteDroneMaintenanceResource(
    DateTimeOffset? MaintenanceDate,
    DateTimeOffset? NextMaintenanceDate
);

/**
 * Body of Post Drone => failure
 * Post/api/v1/drones/{id}/failure
 */
public sealed record ReportDroneFailureResource(
    string Description,
    DateTimeOffset? OccurredAt
);
