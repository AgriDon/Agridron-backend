using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record DroneRegisteredEvent(
    long DroneId,
    string SerialNumber,
    string Model,
    decimal CapacityLiters,
    DateTimeOffset OccurredAt) : IEvent;
