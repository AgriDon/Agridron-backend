using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when a farm has been persisted and its identity is available.
/// </summary>
/// <param name="FarmId">The new farm identifier.</param>
/// <param name="OwnerId">The identifier of the user that owns the farm.</param>
/// <param name="Name">The farm name.</param>
/// <param name="Location">The farm location.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record FarmCreatedEvent(
    long FarmId,
    long OwnerId,
    string Name,
    string Location,
    DateTimeOffset OccurredAt) : IEvent;
