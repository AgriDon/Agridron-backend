using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionStartedEvent(
    long MissionId,
    DateTimeOffset StartedAt,
    DateTimeOffset OccurredAt) : IEvent;
