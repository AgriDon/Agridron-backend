using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionCancelledEvent(
    long MissionId,
    long? DroneId,
    string? Reason,
    DateTimeOffset OccurredAt) : IEvent;
