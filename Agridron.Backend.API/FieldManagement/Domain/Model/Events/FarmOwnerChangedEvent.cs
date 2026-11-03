using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when the owner of a farm changes.
/// </summary>
/// <param name="FarmId">The farm identifier.</param>
/// <param name="OwnerId">The new owner identifier.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record FarmOwnerChangedEvent(
    long FarmId,
    long OwnerId,
    DateTimeOffset OccurredAt) : IEvent;
