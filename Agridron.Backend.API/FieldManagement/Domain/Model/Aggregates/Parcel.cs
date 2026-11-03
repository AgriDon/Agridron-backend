using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Events;
using Agridron.Backend.API.FieldManagement.Domain.Model.Services;
using Agridron.Backend.API.FieldManagement.Domain.Model.Validation;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;

/// <summary>
///     A delimited portion of a farm where a crop is grown and missions are executed.
/// </summary>
public sealed class Parcel : AggregateRoot<ParcelId>
{
    /// <summary>Maximum length of the parcel name.</summary>
    public const int MaxNameLength = 120;

    /// <summary>Maximum length of the parcel image URL.</summary>
    public const int MaxImageUrlLength = 500;

    /// <summary>Maximum area accepted for a parcel, in hectares.</summary>
    public const decimal MaxAreaHectares = 100_000m;

    private readonly bool _isInitializing;
    private bool _creationEventRaised;

    /// <summary>
    ///     Parameterless constructor required by Entity Framework Core materialization.
    /// </summary>
    private Parcel()
    {
    }

    /// <summary>
    ///     Creates a parcel. The area is derived from the geometry, never provided by the client.
    /// </summary>
    /// <param name="farmId">The farm the parcel belongs to.</param>
    /// <param name="cropId">The crop grown in the parcel.</param>
    /// <param name="name">The parcel name.</param>
    /// <param name="geometry">The validated polygon.</param>
    /// <param name="imageUrl">An optional image URL.</param>
    public Parcel(
        FarmId farmId,
        CropId cropId,
        string name,
        GeoJsonPolygon geometry,
        string? imageUrl = null) : base(ParcelId.Empty)
    {
        _isInitializing = true;

        try
        {
            MoveToFarm(farmId);
            AssignCrop(cropId);
            Rename(name);
            RedefineGeometry(geometry);
            ChangeImage(imageUrl);
        }
        finally
        {
            _isInitializing = false;
        }
    }

    /// <summary>The farm the parcel belongs to.</summary>
    public FarmId FarmId { get; private set; }

    /// <summary>The crop grown in the parcel.</summary>
    public CropId CropId { get; private set; }

    /// <summary>The parcel name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The parcel polygon.</summary>
    public GeoJsonPolygon Geometry { get; private set; } = null!;

    /// <summary>The area in hectares, derived from <see cref="Geometry" />.</summary>
    public decimal AreaHectares { get; private set; }

    /// <summary>An optional image URL.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Moves the parcel to another farm.</summary>
    /// <param name="farmId">The new farm identifier.</param>
    public void MoveToFarm(FarmId farmId)
    {
        if (farmId.Value <= 0)
        {
            throw new DomainException(FieldManagementErrors.ParcelFarmInvalid);
        }

        FarmId = farmId;
    }

    /// <summary>Assigns another crop to the parcel.</summary>
    /// <param name="cropId">The new crop identifier.</param>
    public void AssignCrop(CropId cropId)
    {
        if (cropId.Value <= 0)
        {
            throw new DomainException(FieldManagementErrors.ParcelCropInvalid);
        }

        if (!_isInitializing && cropId != CropId)
        {
            AddDomainEvent(new ParcelCropAssignedEvent(Id.Value, cropId.Value, DateTimeOffset.UtcNow));
        }

        CropId = cropId;
    }

    /// <summary>Renames the parcel.</summary>
    /// <param name="name">The new name.</param>
    public void Rename(string name)
    {
        Name = FieldManagementGuard.RequiredText(
            name,
            FieldManagementErrors.ParcelNameRequired,
            MaxNameLength,
            FieldManagementErrors.ParcelNameTooLong);
    }

    /// <summary>
    ///     Replaces the parcel polygon and recalculates its area.
    /// </summary>
    /// <param name="geometry">The new validated polygon.</param>
    /// <exception cref="DomainException">When the resulting area is not within the accepted range.</exception>
    public void RedefineGeometry(GeoJsonPolygon geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var area = GeodesicAreaCalculator.CalculateHectares(geometry);
        if (area <= 0m || area > MaxAreaHectares)
        {
            throw new DomainException(FieldManagementErrors.ParcelAreaOutOfRange);
        }

        if (!_isInitializing && geometry != Geometry)
        {
            AddDomainEvent(new ParcelGeometryUpdatedEvent(
                Id.Value,
                FarmId.Value,
                CropId.Value,
                area,
                geometry.GeoJson,
                DateTimeOffset.UtcNow));
        }

        Geometry = geometry;
        AreaHectares = area;
    }

    /// <summary>Changes or clears the parcel image.</summary>
    /// <param name="imageUrl">The new image URL, or null to clear it.</param>
    public void ChangeImage(string? imageUrl)
    {
        ImageUrl = FieldManagementGuard.OptionalUrl(
            imageUrl,
            MaxImageUrlLength,
            FieldManagementErrors.ParcelImageUrlInvalid);
    }

    /// <summary>
    ///     Raises the creation event once the parcel has been persisted and its identity is known.
    /// </summary>
    /// <remarks>
    ///     The identifier is generated by the database, so the event cannot be raised by the
    ///     constructor. The application layer calls this method after the unit of work commits.
    /// </remarks>
    /// <exception cref="InvalidOperationException">When the parcel has not been persisted yet.</exception>
    public void MarkAsCreated()
    {
        if (_creationEventRaised)
        {
            return;
        }

        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The parcel must be persisted before raising its creation event.");
        }

        AddDomainEvent(new ParcelCreatedEvent(
            Id.Value,
            FarmId.Value,
            CropId.Value,
            Name,
            AreaHectares,
            Geometry.GeoJson,
            DateTimeOffset.UtcNow));

        _creationEventRaised = true;
    }
}
