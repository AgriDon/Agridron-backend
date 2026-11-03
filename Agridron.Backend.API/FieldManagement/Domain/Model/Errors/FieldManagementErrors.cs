using Agridron.Backend.API.Shared.Domain.Model;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Errors;

/// <summary>
///     Error catalog of the Field Management bounded context.
/// </summary>
/// <remarks>
///     Every code is unique inside the bounded context and is part of the HTTP contract
///     documented in <c>docs/field-api.md</c>.
/// </remarks>
public static class FieldManagementErrors
{
    // ---------------------------------------------------------------------
    // Farm
    // ---------------------------------------------------------------------

    public static readonly Error FarmNameRequired =
        Error.Validation("FarmNameRequired", "The farm name is required.");

    public static readonly Error FarmNameTooLong =
        Error.Validation("FarmNameTooLong", "The farm name cannot exceed 120 characters.");

    public static readonly Error FarmLocationRequired =
        Error.Validation("FarmLocationRequired", "The farm location is required.");

    public static readonly Error FarmLocationTooLong =
        Error.Validation("FarmLocationTooLong", "The farm location cannot exceed 200 characters.");

    public static readonly Error FarmOwnerInvalid =
        Error.Validation("FarmOwnerInvalid", "The farm owner identifier must be greater than zero.");

    public static readonly Error FarmImageUrlInvalid =
        Error.Validation("FarmImageUrlInvalid", "The farm image must be an absolute http or https URL.");

    public static Error FarmNotFound(long id)
    {
        return Error.NotFound("FarmNotFound", $"Farm with id {id} was not found.");
    }

    public static Error FarmHasParcels(long id)
    {
        return Error.Conflict("FarmHasParcels", $"Farm with id {id} still has parcels and cannot be deleted.");
    }

    // ---------------------------------------------------------------------
    // Crop
    // ---------------------------------------------------------------------

    public static readonly Error CropNameRequired =
        Error.Validation("CropNameRequired", "The crop name is required.");

    public static readonly Error CropNameTooLong =
        Error.Validation("CropNameTooLong", "The crop name cannot exceed 80 characters.");

    public static readonly Error CropVarietyRequired =
        Error.Validation("CropVarietyRequired", "The crop variety is required.");

    public static readonly Error CropVarietyTooLong =
        Error.Validation("CropVarietyTooLong", "The crop variety cannot exceed 80 characters.");

    public static Error CropNotFound(long id)
    {
        return Error.NotFound("CropNotFound", $"Crop with id {id} was not found.");
    }

    public static Error CropAlreadyExists(string name, string variety)
    {
        return Error.Conflict("CropAlreadyExists", $"A crop named '{name}' with variety '{variety}' already exists.");
    }

    public static Error CropHasParcels(long id)
    {
        return Error.Conflict("CropHasParcels", $"Crop with id {id} is assigned to parcels and cannot be deleted.");
    }

    // ---------------------------------------------------------------------
    // Parcel
    // ---------------------------------------------------------------------

    public static readonly Error ParcelNameRequired =
        Error.Validation("ParcelNameRequired", "The parcel name is required.");

    public static readonly Error ParcelNameTooLong =
        Error.Validation("ParcelNameTooLong", "The parcel name cannot exceed 120 characters.");

    public static readonly Error ParcelImageUrlInvalid =
        Error.Validation("ParcelImageUrlInvalid", "The parcel image must be an absolute http or https URL.");

    public static readonly Error ParcelAreaOutOfRange =
        Error.Validation("ParcelAreaOutOfRange", "The parcel area must be greater than zero and at most 100000 hectares.");

    public static readonly Error ParcelFarmInvalid =
        Error.Validation("ParcelFarmInvalid", "The parcel farm identifier must be greater than zero.");

    public static readonly Error ParcelCropInvalid =
        Error.Validation("ParcelCropInvalid", "The parcel crop identifier must be greater than zero.");

    public static Error ParcelNotFound(long id)
    {
        return Error.NotFound("ParcelNotFound", $"Parcel with id {id} was not found.");
    }

    public static Error ParcelGeometryInvalid(string reason)
    {
        return Error.Validation("ParcelGeometryInvalid", "The parcel geometry is not a valid polygon.", reason);
    }

    public static Error ParcelHasFumigationAreas(long id)
    {
        return Error.Conflict(
            "ParcelHasFumigationAreas",
            $"Parcel with id {id} still has fumigation areas and cannot be deleted.");
    }

    // ---------------------------------------------------------------------
    // Fumigation area
    // ---------------------------------------------------------------------

    public static readonly Error FumigationAreaHasNoArea =
        Error.Validation("FumigationAreaHasNoArea", "The fumigation area must be greater than zero.");

    public static readonly Error FumigationAreaParcelInvalid =
        Error.Validation("FumigationAreaParcelInvalid", "The fumigation area parcel identifier must be greater than zero.");

    public static Error FumigationAreaNotFound(long id)
    {
        return Error.NotFound("FumigationAreaNotFound", $"Fumigation area with id {id} was not found.");
    }

    public static Error FumigationAreaExceedsParcelArea(decimal area, decimal parcelArea)
    {
        return Error.BusinessRule(
            "FumigationAreaExceedsParcelArea",
            "The fumigation area cannot exceed the parcel area.",
            $"Fumigation area: {area} ha. Parcel area: {parcelArea} ha.");
    }

    // ---------------------------------------------------------------------
    // GeoJSON polygon
    // ---------------------------------------------------------------------

    public static readonly Error PolygonGeoJsonRequired =
        Error.Validation("PolygonGeoJsonRequired", "The polygon GeoJSON is required.");

    public static readonly Error InvalidPolygonType =
        Error.Validation("InvalidPolygonType", "The GeoJSON type must be 'Polygon'.");

    public static readonly Error PolygonCoordinatesMissing =
        Error.Validation("PolygonCoordinatesMissing", "The GeoJSON must contain a coordinates array.");

    public static readonly Error PolygonTooFewPoints =
        Error.Validation("PolygonTooFewPoints", "A polygon ring must contain at least four positions.");

    public static readonly Error PolygonRingNotClosed =
        Error.Validation("PolygonRingNotClosed", "The first and last positions of a polygon ring must match.");

    public static readonly Error CoordinateOutOfRange =
        Error.Validation("CoordinateOutOfRange", "Longitude must be between -180 and 180 and latitude between -90 and 90.");

    public static Error PolygonGeoJsonInvalid(string reason)
    {
        return Error.Validation("PolygonGeoJsonInvalid", "The GeoJSON could not be parsed.", reason);
    }
}
