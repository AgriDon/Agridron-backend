using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Events;


/// <summary>
///  Raised when a nozzle changes its condition (good / worn).
/// </summary>
/// <param name="NozzleId"></param>
/// <param name="OldStatus"></param>
/// <param name="NewStatus"></param>
/// <param name="OccurredAt"></param>
public sealed record NozzleStatusChangedEvent(
    long NozzleId,
    NozzleStatus OldStatus,
    NozzleStatus NewStatus,
    DateTimeOffset OccurredAt) : IEvent;
