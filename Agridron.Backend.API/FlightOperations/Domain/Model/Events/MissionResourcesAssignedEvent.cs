using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionResourcesAssignedEvent(
    long MissionId,
    long DroneId,
    string? TreatmentType,
    string? ProductName,
    decimal? ProductDose,
    DateTimeOffset OccurredAt) : IEvent;
