using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Transform;

public static class MissionAssembler
{
    public static MissionResource ToResourceFrom(Mission entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        FlightParametersResource? flightParamsResource = null;
        if (entity.FlightParameters is not null)
        {
            flightParamsResource = new FlightParametersResource(
                entity.FlightParameters.AltitudeMeters,
                entity.FlightParameters.SpeedMetersPerSecond,
                entity.FlightParameters.SprayRateLitersPerHectare,
                entity.FlightParameters.OverlapPercentage);
        }

        GeoCoordinateResource? lastPauseResource = null;
        if (entity.LastPausePoint is not null)
        {
            lastPauseResource = new GeoCoordinateResource(
                entity.LastPausePoint.Latitude,
                entity.LastPausePoint.Longitude);
        }

        return new MissionResource(
            entity.Id.Value,
            entity.Code,
            entity.FarmId,
            entity.ParcelId,
            entity.DroneId?.Value,
            entity.ChemicalId,
            entity.OperatorId,
            entity.FarmArea,
            entity.CropType,
            entity.OperatorName,
            entity.TreatmentType,
            entity.ProductName,
            entity.ProductDose,
            entity.Notes,
            entity.Status.ToString(),
            entity.ScheduledAt,
            entity.PlannedAreaHectares,
            entity.TreatedAreaHectares,
            entity.Progress,
            flightParamsResource,
            lastPauseResource,
            entity.StartedAt,
            entity.CompletedAt,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static CreateMissionCommand ToCommandFrom(CreateMissionResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new CreateMissionCommand(
            resource.Code,
            resource.FarmId,
            resource.ParcelId,
            resource.FarmArea,
            resource.CropType,
            resource.ScheduledAt,
            resource.PlannedAreaHectares,
            resource.OperatorName,
            resource.TreatmentType,
            resource.ProductName,
            resource.ProductDose,
            resource.Notes,
            resource.DroneId,
            resource.Progress);
    }

    public static UpdateMissionCommand ToCommandFrom(long id, UpdateMissionResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new UpdateMissionCommand(
            id,
            resource.FarmId,
            resource.ParcelId,
            resource.FarmArea,
            resource.CropType,
            resource.ScheduledAt,
            resource.PlannedAreaHectares,
            resource.OperatorName,
            resource.TreatmentType,
            resource.ProductName,
            resource.ProductDose,
            resource.Notes);
    }

    public static AssignMissionResourcesCommand ToCommandFrom(long missionId, AssignMissionResourcesResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new AssignMissionResourcesCommand(
            missionId,
            resource.DroneId,
            resource.TreatmentType,
            resource.ProductName,
            resource.ProductDose,
            resource.ChemicalId);
    }

    public static AssignMissionOperatorCommand ToCommandFrom(long missionId, AssignMissionOperatorResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new AssignMissionOperatorCommand(
            missionId,
            resource.OperatorName,
            resource.OperatorId);
    }

    public static ConfigureFlightParametersCommand ToCommandFrom(long missionId, ConfigureFlightParametersResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new ConfigureFlightParametersCommand(
            missionId,
            resource.AltitudeMeters,
            resource.SpeedMetersPerSecond,
            resource.SprayRateLitersPerHectare,
            resource.OverlapPercentage);
    }
}
