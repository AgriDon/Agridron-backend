using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionCompletedEvent(
    long MissionId,
    long? DroneId,
    decimal TreatedAreaHectares,
    DateTimeOffset CompletedAt,
    DateTimeOffset OccurredAt) : IEvent;
