using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when the name of a farm changes.
/// </summary>
/// <param name="FarmId">The farm identifier.</param>
/// <param name="Name">The new farm name.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record FarmRenamedEvent(
    long FarmId,
    string Name,
    DateTimeOffset OccurredAt) : IEvent;
