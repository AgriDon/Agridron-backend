using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Transform;

public static class OperationStatusAssembler
{
    public static OperationStatusResource ToResourceFrom(OperationStatus entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new OperationStatusResource(
            entity.Id.Value,
            entity.MissionId?.Value,
            entity.DroneId?.Value,
            entity.Location.Latitude,
            entity.Location.Longitude,
            entity.AltitudeMeters,
            entity.SpeedMetersPerSecond,
            entity.BatteryLevel,
            entity.TankVolumeLiters,
            entity.Status,
            entity.Timestamp,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static CreateOperationStatusCommand ToCommandFrom(CreateOperationStatusResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new CreateOperationStatusCommand(
            resource.Latitude,
            resource.Longitude,
            resource.Status,
            resource.Timestamp,
            resource.MissionId,
            resource.DroneId,
            resource.AltitudeMeters,
            resource.SpeedMetersPerSecond,
            resource.BatteryLevel,
            resource.TankVolumeLiters);
    }

    public static UpdateOperationStatusCommand ToCommandFrom(long id, UpdateOperationStatusResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new UpdateOperationStatusCommand(
            id,
            resource.Latitude,
            resource.Longitude,
            resource.Status);
    }
}
