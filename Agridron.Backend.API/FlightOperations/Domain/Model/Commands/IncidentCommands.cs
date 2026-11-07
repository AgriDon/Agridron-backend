namespace Agridron.Backend.API.FlightOperations.Domain.Model.Commands;

public sealed record CreateIncidentCommand(
    string Type,
    string Description,
    DateTimeOffset? OccurredAt = null,
    long? MissionId = null,
    long? DroneId = null,
    string? Severity = null);

public sealed record UpdateIncidentCommand(
    long Id,
    string Type,
    string Description,
    string Severity,
    DateTimeOffset OccurredAt);

public sealed record DeleteIncidentCommand(long Id);
