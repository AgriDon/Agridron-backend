using Agridron.Backend.API.Shared.Domain.Model;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Errors;

/// <summary>
///     Error catalog of the Flight Operations bounded context.
/// </summary>
public static class FlightOperationsErrors
{
    // ---------------------------------------------------------------------
    // Drone Errors
    // ---------------------------------------------------------------------

    public static readonly Error DroneSerialNumberRequired =
        Error.Validation("DroneSerialNumberRequired", "The drone serial number is required.");

    public static readonly Error DroneSerialNumberTooLong =
        Error.Validation("DroneSerialNumberTooLong", "The drone serial number cannot exceed 60 characters.");

    public static readonly Error DroneModelRequired =
        Error.Validation("DroneModelRequired", "The drone model is required.");

    public static readonly Error DroneModelTooLong =
        Error.Validation("DroneModelTooLong", "The drone model cannot exceed 100 characters.");

    public static readonly Error DroneCapacityInvalid =
        Error.Validation("DroneCapacityInvalid", "The drone capacity in liters must be greater than zero.");

    public static readonly Error DroneImageUrlInvalid =
        Error.Validation("DroneImageUrlInvalid", "The drone image URL must be an absolute http or https URL.");

    public static Error DroneNotFound(long id) =>
        Error.NotFound("DroneNotFound", $"Drone with id {id} was not found.");

    public static Error DroneSerialNumberAlreadyExists(string serialNumber) =>
        Error.Conflict("DroneSerialNumberAlreadyExists", $"A drone with serial number '{serialNumber}' already exists.");

    public static Error DroneNotAvailable(long id, string status) =>
        Error.Conflict("DroneNotAvailable", $"Drone with id {id} is in status '{status}' and cannot be assigned.");

    public static Error DroneBusy(long id, string status) =>
        Error.Conflict("DroneBusy", $"Drone with id {id} is currently '{status}' and cannot be deleted or modified.");

    public static Error DroneFailureNotAllowed(long id, string status) =>
        Error.Conflict("DroneFailureNotAllowed", $"A failure cannot be reported for drone with id {id} while it is '{status}'.");

    public static Error DroneNotInMaintenance(long id, string status) =>
        Error.Conflict("DroneNotInMaintenance", $"Drone with id {id} is '{status}'; only a drone in maintenance or out of service can complete a maintenance.");

    public static readonly Error DroneFailureDescriptionRequired =
        Error.Validation("DroneFailureDescriptionRequired", "The failure description is required.");

    // ---------------------------------------------------------------------
    // Mission Errors
    // ---------------------------------------------------------------------

    public static readonly Error MissionCodeRequired =
        Error.Validation("MissionCodeRequired", "The mission code is required.");

    public static readonly Error MissionCodeTooLong =
        Error.Validation("MissionCodeTooLong", "The mission code cannot exceed 30 characters.");

    public static readonly Error MissionFarmAreaTooLong =
        Error.Validation("MissionFarmAreaTooLong", "The farm area name cannot exceed 120 characters.");

    public static readonly Error MissionCropTypeTooLong =
        Error.Validation("MissionCropTypeTooLong", "The crop type cannot exceed 80 characters.");

    public static readonly Error MissionOperatorNameTooLong =
        Error.Validation("MissionOperatorNameTooLong", "The operator name cannot exceed 120 characters.");

    public static readonly Error MissionTreatmentTypeTooLong =
        Error.Validation("MissionTreatmentTypeTooLong", "The treatment type cannot exceed 60 characters.");

    public static readonly Error MissionProductNameTooLong =
        Error.Validation("MissionProductNameTooLong", "The product name cannot exceed 100 characters.");

    public static readonly Error MissionProductDoseInvalid =
        Error.Validation("MissionProductDoseInvalid", "The product dose must be greater than or equal to zero.");

    public static readonly Error MissionNotesTooLong =
        Error.Validation("MissionNotesTooLong", "Mission notes cannot exceed 1000 characters.");

    public static readonly Error MissionProgressInvalid =
        Error.Validation("MissionProgressInvalid", "The progress must be between 0 and 100 percent.");

    public static readonly Error MissionPlannedAreaOutOfRange =
        Error.Validation("MissionPlannedAreaOutOfRange", "Planned area must be greater than zero and at most 100,000 hectares.");

    public static readonly Error MissionTreatedAreaOutOfRange =
        Error.Validation("MissionTreatedAreaOutOfRange", "Treated area must be greater than or equal to zero and at most 100,000 hectares.");

    public static Error MissionNotFound(long id) =>
        Error.NotFound("MissionNotFound", $"Mission with id {id} was not found.");

    public static Error MissionCodeAlreadyExists(string code) =>
        Error.Conflict("MissionCodeAlreadyExists", $"A mission with code '{code}' already exists.");

    public static Error MissionParcelInvalid(long parcelId) =>
        Error.Validation("MissionParcelInvalid", $"Parcel identifier {parcelId} is invalid.");

    public static Error MissionParcelNotFound(long parcelId) =>
        Error.NotFound("MissionParcelNotFound", $"Parcel with id {parcelId} was not found or is inactive.");

    public static readonly Error MissionDroneRequiredForApproval =
        Error.BusinessRule("MissionDroneRequiredForApproval", "A drone must be assigned to the mission before it can be approved.");

    public static Error MissionInvalidStatusTransition(string currentStatus, string targetAction) =>
        Error.BusinessRule("MissionInvalidStatusTransition", $"Cannot execute '{targetAction}' while mission is in status '{currentStatus}'.");

    public static Error MissionCannotBeCancelled(string status) =>
        Error.BusinessRule("MissionCannotBeCancelled", $"Mission in status '{status}' cannot be cancelled.");

    public static Error MissionInProgress(long missionId) =>
        Error.Conflict("MissionInProgress", $"Mission with id {missionId} is currently in progress and cannot be deleted.");

    public static Error FarmNotFound(long farmId) =>
        Error.NotFound("FarmNotFound", $"Farm with id {farmId} was not found.");

    public static Error ParcelNotFound(long parcelId) =>
        Error.NotFound("ParcelNotFound", $"Parcel with id {parcelId} was not found.");

    public static Error ParcelDoesNotBelongToFarm(long parcelId, long farmId) =>
        Error.Conflict("ParcelDoesNotBelongToFarm", $"Parcel with id {parcelId} does not belong to farm with id {farmId}.");

    // ---------------------------------------------------------------------
    // Flight Parameters Errors
    // ---------------------------------------------------------------------

    public static readonly Error FlightAltitudeOutOfRange =
        Error.Validation("FlightAltitudeOutOfRange", "Flight altitude must be between 1.0 and 30.0 meters.");

    public static readonly Error FlightSpeedOutOfRange =
        Error.Validation("FlightSpeedOutOfRange", "Flight speed must be between 1.0 and 20.0 meters per second.");

    public static readonly Error SprayRateOutOfRange =
        Error.Validation("SprayRateOutOfRange", "Spray rate must be between 1.0 and 100.0 liters per hectare.");

    public static readonly Error OverlapPercentageOutOfRange =
        Error.Validation("OverlapPercentageOutOfRange", "Overlap percentage must be between 0.0 and 80.0 percent.");

    // ---------------------------------------------------------------------
    // GeoCoordinate Errors
    // ---------------------------------------------------------------------

    public static readonly Error LatitudeOutOfRange =
        Error.Validation("LatitudeOutOfRange", "Latitude must be between -90.0 and 90.0 degrees.");

    public static readonly Error LongitudeOutOfRange =
        Error.Validation("LongitudeOutOfRange", "Longitude must be between -180.0 and 180.0 degrees.");

    // ---------------------------------------------------------------------
    // Incident Errors
    // ---------------------------------------------------------------------

    public static readonly Error IncidentTypeRequired =
        Error.Validation("IncidentTypeRequired", "The incident type is required.");

    public static readonly Error IncidentTypeTooLong =
        Error.Validation("IncidentTypeTooLong", "The incident type cannot exceed 60 characters.");

    public static readonly Error IncidentDescriptionRequired =
        Error.Validation("IncidentDescriptionRequired", "The incident description is required.");

    public static readonly Error IncidentDescriptionTooLong =
        Error.Validation("IncidentDescriptionTooLong", "The incident description cannot exceed 500 characters.");

    public static Error IncidentNotFound(long id) =>
        Error.NotFound("IncidentNotFound", $"Incident with id {id} was not found.");

    // ---------------------------------------------------------------------
    // Operation Status Errors
    // ---------------------------------------------------------------------

    public static readonly Error OperationStatusStateRequired =
        Error.Validation("OperationStatusStateRequired", "The operation status state description is required.");

    public static readonly Error OperationStatusStateTooLong =
        Error.Validation("OperationStatusStateTooLong", "The operation status state cannot exceed 60 characters.");

    public static Error OperationStatusNotFound(long id) =>
        Error.NotFound("OperationStatusNotFound", $"Operation status with id {id} was not found.");
}
