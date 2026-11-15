using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Transform;

public static class DroneAssembler
{
    public static string ToExternalString(this DroneStatus status) => status switch
    {
        DroneStatus.Available => "AVAILABLE",
        DroneStatus.Assigned => "ASSIGNED",
        DroneStatus.InFlight => "IN_FLIGHT",
        DroneStatus.Paused => "PAUSED",
        DroneStatus.Maintenance => "MAINTENANCE",
        DroneStatus.OutOfService => "OUT_OF_SERVICE",
        _ => status.ToString().ToUpperInvariant()
    };

    public static DroneResource ToResourceFrom(Drone entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new DroneResource(
            entity.Id.Value,
            entity.SerialNumber,
            entity.Model,
            entity.CapacityLiters,
            entity.CapacityLiters,
            entity.Status.ToExternalString(),
            entity.BatteryLevel,
            entity.BatteryLevel,
            entity.FlightHours,
            entity.NozzleId,
            entity.LastMaintenanceDate,
            entity.NextMaintenanceDate,
            entity.ImageUrl,
            entity.ImageUrl,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static CreateDroneCommand ToCommandFrom(CreateDroneResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var capacity = resource.CapacityLiters ?? resource.Capacity ?? 0m;
        var imageUrl = resource.ImageUrl ?? resource.Urlimg;

        return new CreateDroneCommand(
            resource.SerialNumber,
            resource.Model,
            capacity,
            resource.NozzleId,
            imageUrl);
    }

    public static UpdateDroneCommand ToCommandFrom(long id, UpdateDroneResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var capacity = resource.CapacityLiters ?? resource.Capacity ?? 0m;
        var imageUrl = resource.ImageUrl ?? resource.Urlimg;

        return new UpdateDroneCommand(
            id,
            resource.Model,
            capacity,
            resource.NozzleId,
            imageUrl,
            resource.LastMaintenanceDate,
            resource.NextMaintenanceDate);
    }
}
