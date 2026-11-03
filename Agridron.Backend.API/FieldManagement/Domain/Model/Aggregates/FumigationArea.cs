using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Services;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;

/// <summary>
///     The portion of a parcel that is actually treated during a fumigation mission.
/// </summary>
public sealed class FumigationArea : AggregateRoot<FumigationAreaId>
{
    /// <summary>
    ///     Parameterless constructor required by Entity Framework Core materialization.
    /// </summary>
    private FumigationArea()
    {
    }

    /// <summary>
    ///     Creates a fumigation area. The area is derived from the geometry.
    /// </summary>
    /// <param name="parcelId">The parcel the area belongs to.</param>
    /// <param name="geometry">The validated polygon.</param>
    public FumigationArea(ParcelId parcelId, GeoJsonPolygon geometry) : base(FumigationAreaId.Empty)
    {
        MoveToParcel(parcelId);
        RedefineGeometry(geometry);
    }

    /// <summary>The parcel the area belongs to.</summary>
    public ParcelId ParcelId { get; private set; }

    /// <summary>The area polygon.</summary>
    public GeoJsonPolygon Geometry { get; private set; } = null!;

    /// <summary>The area in hectares, derived from <see cref="Geometry" />.</summary>
    public decimal AreaHectares { get; private set; }

    /// <summary>Moves the area to another parcel.</summary>
    /// <param name="parcelId">The new parcel identifier.</param>
    public void MoveToParcel(ParcelId parcelId)
    {
        if (parcelId.Value <= 0)
        {
            throw new DomainException(FieldManagementErrors.FumigationAreaParcelInvalid);
        }

        ParcelId = parcelId;
    }

    /// <summary>
    ///     Replaces the area polygon and recalculates its area.
    /// </summary>
    /// <param name="geometry">The new validated polygon.</param>
    /// <exception cref="DomainException">When the polygon has no area.</exception>
    public void RedefineGeometry(GeoJsonPolygon geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var area = GeodesicAreaCalculator.CalculateHectares(geometry);
        if (area <= 0m)
        {
            throw new DomainException(FieldManagementErrors.FumigationAreaHasNoArea);
        }

        Geometry = geometry;
        AreaHectares = area;
    }

    /// <summary>
    ///     Verifies that the area does not exceed the parcel it belongs to.
    /// </summary>
    /// <param name="parcelAreaHectares">The area of the parcel, supplied by the application layer.</param>
    /// <exception cref="DomainException">When the area exceeds the parcel area.</exception>
    public void EnsureFitsWithin(decimal parcelAreaHectares)
    {
        if (parcelAreaHectares <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parcelAreaHectares),
                "The parcel area must be greater than zero.");
        }

        if (AreaHectares > parcelAreaHectares)
        {
            throw new DomainException(
                FieldManagementErrors.FumigationAreaExceedsParcelArea(AreaHectares, parcelAreaHectares));
        }
    }
}
