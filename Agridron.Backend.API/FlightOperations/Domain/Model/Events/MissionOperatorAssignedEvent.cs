using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionOperatorAssignedEvent(
    long MissionId,
    string OperatorName,
    long? OperatorId,
    DateTimeOffset OccurredAt) : IEvent;
