using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record DroneStatusChangedEvent(
    long DroneId,
    DroneStatus OldStatus,
    DroneStatus NewStatus,
    DateTimeOffset OccurredAt) : IEvent;
