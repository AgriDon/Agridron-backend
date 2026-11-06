using System.Text.Json.Serialization;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

public sealed record OperationStatusResource(
    long Id,
    long? MissionId,
    long? DroneId,
    decimal Latitude,
    decimal Longitude,
    decimal? AltitudeMeters,
    decimal? SpeedMetersPerSecond,
    decimal? BatteryLevel,
    decimal? TankVolumeLiters,
    string Status,
    DateTimeOffset Timestamp,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateOperationStatusResource(
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Latitude,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Longitude,
    string Status,
    DateTimeOffset? Timestamp,
    long? MissionId,
    long? DroneId,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? AltitudeMeters,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? SpeedMetersPerSecond,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? BatteryLevel,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? TankVolumeLiters);

public sealed record UpdateOperationStatusResource(
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Latitude,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Longitude,
    string Status);
