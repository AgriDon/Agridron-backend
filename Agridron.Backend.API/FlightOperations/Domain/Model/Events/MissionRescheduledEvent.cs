using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Events;


/// <summary>
///  Raised when a mission that has not taken off yet changes its scheduled date
/// </summary>
/// <param name="MissionId"></param>
/// <param name="PreviousScheduleAt"></param>
/// <param name="NewScheduleAt"></param>
/// <param name="NextScheduleAt"></param>
public sealed record MissionRescheduledEvent(
    long MissionId,
    DateTimeOffset PreviousScheduleAt,
    DateTimeOffset NewScheduleAt,
    DateTimeOffset NextScheduleAt
) : IEvent;
