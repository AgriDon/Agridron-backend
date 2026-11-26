using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;


/// <summary>
///  Raised when a mission that has not taken off yet changes its scheduled date
/// </summary>
/// <param name="MissionId"></param>
/// <param name="PreviousScheduledAt"></param>
/// <param name="NewScheduledAt"></param>
/// <param name="OccurredAt"></param>
public sealed record MissionRescheduledEvent(
    long MissionId,
    DateTimeOffset PreviousScheduledAt,
    DateTimeOffset NewScheduledAt,
    DateTimeOffset OccurredAt
) : IEvent;
