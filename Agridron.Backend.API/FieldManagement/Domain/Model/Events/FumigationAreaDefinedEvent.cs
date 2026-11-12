using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when a fumigation area has been persisted and its identity is available.
/// </summary>
/// <param name="FumigationAreaId">The new fumigation area identifier.</param>
/// <param name="ParcelId">The parcel the area belongs to.</param>
/// <param name="AreaHectares">The area derived from the geometry, in hectares.</param>
/// <param name="Geometry">The canonical GeoJSON polygon.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record FumigationAreaDefinedEvent(
    long FumigationAreaId,
    long ParcelId,
    decimal AreaHectares,
    string Geometry,
    DateTimeOffset OccurredAt) : IEvent;
