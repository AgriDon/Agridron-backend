using System.Text.Json.Serialization;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

public sealed record FlightParametersResource(
    decimal AltitudeMeters,
    decimal SpeedMetersPerSecond,
    decimal SprayRateLitersPerHectare,
    decimal OverlapPercentage);

public sealed record GeoCoordinateResource(
    decimal Latitude,
    decimal Longitude);

public sealed record MissionResource(
    long Id,
    string Code,
    long? FarmId,
    long? ParcelId,
    long? DroneId,
    long? ChemicalId,
    long? OperatorId,
    string FarmArea,
    string CropType,
    string? OperatorName,
    string? TreatmentType,
    string? ProductName,
    decimal? ProductDose,
    string? Notes,
    string Status,
    DateTimeOffset ScheduledAt,
    decimal PlannedAreaHectares,
    decimal TreatedAreaHectares,
    int Progress,
    FlightParametersResource? FlightParameters,
    GeoCoordinateResource? LastPausePoint,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    bool IsActive,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateMissionResource(
    string? Code,
    long? FarmId,
    long? ParcelId,
    string? FarmArea,
    string? CropType,
    DateTimeOffset? ScheduledAt,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? PlannedAreaHectares,
    string? OperatorName,
    string? TreatmentType,
    string? ProductName,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? ProductDose,
    string? Notes,
    long? DroneId,
    int? Progress);

public sealed record UpdateMissionResource(
    long? FarmId,
    long? ParcelId,
    string? FarmArea,
    string? CropType,
    DateTimeOffset? ScheduledAt,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? PlannedAreaHectares,
    string? OperatorName,
    string? TreatmentType,
    string? ProductName,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? ProductDose,
    string? Notes);

public sealed record AssignMissionResourcesResource(
    long DroneId,
    string? TreatmentType,
    string? ProductName,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? ProductDose,
    long? ChemicalId);

public sealed record AssignMissionOperatorResource(
    string OperatorName,
    long? OperatorId);

public sealed record ConfigureFlightParametersResource(
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal AltitudeMeters,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal SpeedMetersPerSecond,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal SprayRateLitersPerHectare,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal OverlapPercentage);

public sealed record ApproveMissionResource();

public sealed record StartMissionResource(
    DateTimeOffset? StartedAt);

public sealed record PauseMissionResource(
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? Latitude,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? Longitude,
    string? Reason);

public sealed record ResumeMissionResource();

public sealed record CompleteMissionResource(
    DateTimeOffset? CompletedAt,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal? TreatedAreaHectares,
    string? ClosingNotes);

public sealed record CancelMissionResource(
    string? Reason);
