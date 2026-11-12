using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when a crop has been persisted and its identity is available.
/// </summary>
/// <param name="CropId">The new crop identifier.</param>
/// <param name="Name">The crop name.</param>
/// <param name="Variety">The crop variety.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record CropCreatedEvent(
    long CropId,
    string Name,
    string Variety,
    DateTimeOffset OccurredAt) : IEvent;
