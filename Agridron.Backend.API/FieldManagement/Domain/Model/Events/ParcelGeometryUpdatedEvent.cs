using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Events;

/// <summary>
///     Raised when the polygon of a parcel changes and, therefore, its area.
/// </summary>
/// <param name="ParcelId">The parcel identifier.</param>
/// <param name="FarmId">The farm the parcel belongs to.</param>
/// <param name="CropId">The crop grown in the parcel.</param>
/// <param name="AreaHectares">The new area in hectares.</param>
/// <param name="Geometry">The canonical GeoJSON polygon.</param>
/// <param name="OccurredAt">The instant the event happened.</param>
public sealed record ParcelGeometryUpdatedEvent(
    long ParcelId,
    long FarmId,
    long CropId,
    decimal AreaHectares,
    string Geometry,
    DateTimeOffset OccurredAt) : IEvent;
