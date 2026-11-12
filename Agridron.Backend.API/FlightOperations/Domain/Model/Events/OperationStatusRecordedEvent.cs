using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record OperationStatusRecordedEvent(
    long OperationStatusId,
    long? MissionId,
    long? DroneId,
    decimal Latitude,
    decimal Longitude,
    string Status,
    DateTimeOffset Timestamp) : IEvent;
