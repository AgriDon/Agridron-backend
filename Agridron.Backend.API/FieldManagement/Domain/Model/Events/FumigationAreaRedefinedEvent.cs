using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when the polygon of a fumigation area changes and, therefore, its area.
/// </summary>
/// <param name="FumigationAreaId">The fumigation area identifier.</param>
/// <param name="ParcelId">The parcel the area belongs to.</param>
/// <param name="AreaHectares">The new area in hectares.</param>
/// <param name="Geometry">The canonical GeoJSON polygon.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record FumigationAreaRedefinedEvent(
    long FumigationAreaId,
    long ParcelId,
    decimal AreaHectares,
    string Geometry,
    DateTimeOffset OccurredAt) : IEvent;
