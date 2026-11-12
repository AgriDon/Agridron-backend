using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionCreatedEvent(
    long MissionId,
    string Code,
    long? FarmId,
    long? ParcelId,
    DateTimeOffset ScheduledAt,
    decimal PlannedAreaHectares,
    DateTimeOffset OccurredAt) : IEvent;
