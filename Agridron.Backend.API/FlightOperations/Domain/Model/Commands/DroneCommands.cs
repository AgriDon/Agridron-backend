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
