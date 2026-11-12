using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record MissionPausedEvent(
    long MissionId,
    decimal? Latitude,
    decimal? Longitude,
    DateTimeOffset OccurredAt) : IEvent;
