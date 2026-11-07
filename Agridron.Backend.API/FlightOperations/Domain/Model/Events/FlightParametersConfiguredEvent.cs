using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;

public sealed record FlightParametersConfiguredEvent(
    long MissionId,
    decimal AltitudeMeters,
    decimal SpeedMps,
    decimal SprayRateLitersPerHectare,
    decimal OverlapPercentage,
    DateTimeOffset OccurredAt) : IEvent;
