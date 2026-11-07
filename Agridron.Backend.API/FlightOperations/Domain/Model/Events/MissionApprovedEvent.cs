using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionApprovedEvent(
    long MissionId,
    long DroneId,
    DateTimeOffset OccurredAt) : IEvent;
