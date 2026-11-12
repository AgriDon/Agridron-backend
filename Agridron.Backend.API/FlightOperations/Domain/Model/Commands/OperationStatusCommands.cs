namespace Agridron.Backend.API.FlightOperations.Domain.Model.Commands;

public sealed record CreateOperationStatusCommand(
    decimal Latitude,
    decimal Longitude,
    string Status,
    DateTimeOffset? Timestamp = null,
    long? MissionId = null,
    long? DroneId = null,
    decimal? AltitudeMeters = null,
    decimal? SpeedMetersPerSecond = null,
    decimal? BatteryLevel = null,
    decimal? TankVolumeLiters = null);

public sealed record UpdateOperationStatusCommand(
    long Id,
    decimal Latitude,
    decimal Longitude,
    string Status);

public sealed record DeleteOperationStatusCommand(long Id);
