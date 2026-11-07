using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when a crop is assigned to a parcel.
/// </summary>
/// <param name="ParcelId">The parcel identifier.</param>
/// <param name="CropId">The new crop identifier.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record ParcelCropAssignedEvent(
    long ParcelId,
    long CropId,
    DateTimeOffset OccurredAt) : IEvent;
