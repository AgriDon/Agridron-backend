using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record IncidentReportedEvent(
    long IncidentId,
    long? MissionId,
    long? DroneId,
    string Type,
    string Severity,
    string Description,
    DateTimeOffset OccurredAt) : IEvent;
