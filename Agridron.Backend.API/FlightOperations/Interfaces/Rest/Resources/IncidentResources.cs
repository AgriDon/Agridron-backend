namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

public sealed record IncidentResource(
    long Id,
    string Type,
    string Description,
    DateTimeOffset OccurredAt,
    long? MissionId,
    long? DroneId,
    string Severity,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateIncidentResource(
    string Type,
    string Description,
    DateTimeOffset? OccurredAt,
    long? MissionId,
    long? DroneId,
    string? Severity);

public sealed record UpdateIncidentResource(
    string Type,
    string Description,
    string Severity,
    DateTimeOffset OccurredAt);
