using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when a parcel has been persisted and its identity is available.
/// </summary>
/// <param name="ParcelId">The new parcel identifier.</param>
/// <param name="FarmId">The farm the parcel belongs to.</param>
/// <param name="CropId">The crop grown in the parcel.</param>
/// <param name="Name">The parcel name.</param>
/// <param name="AreaHectares">The area derived from the geometry, in hectares.</param>
/// <param name="Geometry">The canonical GeoJSON polygon.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record ParcelCreatedEvent(
    long ParcelId,
    long FarmId,
    long CropId,
    string Name,
    decimal AreaHectares,
    string Geometry,
    DateTimeOffset OccurredAt) : IEvent;
