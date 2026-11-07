namespace Agridron.Backend.API.FlightOperations.Domain.Model.Commands;

public sealed record CreateMissionCommand(
    string? Code,
    long? FarmId,
    long? ParcelId,
    string? FarmArea,
    string? CropType,
    DateTimeOffset? ScheduledAt,
    decimal? PlannedAreaHectares,
    string? OperatorName,
    string? TreatmentType,
    string? ProductName,
    decimal? ProductDose,
    string? Notes,
    long? DroneId,
    int? Progress);

public sealed record UpdateMissionCommand(
    long Id,
    long? FarmId,
    long? ParcelId,
    string? FarmArea,
    string? CropType,
    DateTimeOffset? ScheduledAt,
    decimal? PlannedAreaHectares,
    string? OperatorName,
    string? TreatmentType,
    string? ProductName,
    decimal? ProductDose,
    string? Notes);

public sealed record DeleteMissionCommand(long Id);

public sealed record AssignMissionResourcesCommand(
    long MissionId,
    long DroneId,
    string? TreatmentType,
    string? ProductName,
    decimal? ProductDose,
    long? ChemicalId = null);

public sealed record AssignMissionOperatorCommand(
    long MissionId,
    string OperatorName,
    long? OperatorId = null);

public sealed record ConfigureFlightParametersCommand(
    long MissionId,
    decimal AltitudeMeters,
    decimal SpeedMetersPerSecond,
    decimal SprayRateLitersPerHectare,
    decimal OverlapPercentage);

public sealed record ApproveMissionCommand(long MissionId);

public sealed record StartMissionCommand(long MissionId, DateTimeOffset? StartedAt = null);

public sealed record PauseMissionCommand(long MissionId, decimal? Latitude = null, decimal? Longitude = null);

public sealed record ResumeMissionCommand(long MissionId);

public sealed record CompleteMissionCommand(
    long MissionId,
    DateTimeOffset? CompletedAt = null,
    decimal? TreatedAreaHectares = null,
    string? ClosingNotes = null);

public sealed record CancelMissionCommand(long MissionId, string? Reason = null);
