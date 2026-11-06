using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record FlightAlertTriggeredEvent(
    long MissionId,
    long? DroneId,
    string AlertType,
    string Severity,
    string Message,
    DateTimeOffset OccurredAt) : IEvent;
