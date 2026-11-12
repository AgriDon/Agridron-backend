using Agridron.Backend.API.FlightOperations.Application.CommandServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices;

internal sealed class OperationStatusCommandService(
    IOperationStatusRepository operationStatusRepository,
    IMissionRepository missionRepository,
    IDroneRepository droneRepository,
    IIncidentRepository incidentRepository,
    IUnitOfWork unitOfWork) : IOperationStatusCommandService
{
    public async Task<Result<OperationStatus>> Handle(CreateOperationStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var location = GeoCoordinate.Create(command.Latitude, command.Longitude);

            Mission? mission = null;
            MissionId? missionId = null;
            if (command.MissionId.HasValue)
            {
                mission = await missionRepository.FindByIdAsync(command.MissionId.Value, cancellationToken);
                if (mission is null || !mission.IsActive)
                {
                    return Result<OperationStatus>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId.Value));
                }

                missionId = new MissionId(command.MissionId.Value);
            }

            DroneId? droneId = null;
            var targetDroneId = command.DroneId ?? mission?.DroneId?.Value;
            Drone? drone = null;

            if (targetDroneId.HasValue)
            {
                drone = await droneRepository.FindByIdAsync(targetDroneId.Value, cancellationToken);
                if (drone is not null && drone.IsActive)
                {
                    droneId = new DroneId(drone.Id.Value);

                    if (command.BatteryLevel.HasValue)
                    {
                        drone.UpdateBatteryLevel(command.BatteryLevel.Value);
                    }
                }
            }

            var timestamp = command.Timestamp ?? DateTimeOffset.UtcNow;

            var operationStatus = new OperationStatus(
                location,
                command.Status,
                timestamp,
                missionId,
                droneId,
                command.AltitudeMeters,
                command.SpeedMetersPerSecond,
                command.BatteryLevel,
                command.TankVolumeLiters);

            await operationStatusRepository.AddAsync(operationStatus, cancellationToken);

            // Automatic Anomaly Detection (TR-003: Alertas por telemetría fuera de rango)
            if (command.BatteryLevel.HasValue && command.BatteryLevel.Value < 15m)
            {
                var batteryIncident = new Incident(
                    "Batería Crítica",
                    $"Nivel de batería crítico reportado a {command.BatteryLevel.Value:F1}% en telemetría.",
                    timestamp,
                    missionId,
                    droneId,
                    command.BatteryLevel.Value < 10m ? IncidentSeverity.Critical : IncidentSeverity.High);

                await incidentRepository.AddAsync(batteryIncident, cancellationToken);
            }

            if (mission?.FlightParameters is not null)
            {
                var maxAltitudeDeviation = 15m;
                if (command.AltitudeMeters.HasValue &&
                    Math.Abs(command.AltitudeMeters.Value - mission.FlightParameters.AltitudeMeters) > maxAltitudeDeviation)
                {
                    var altitudeIncident = new Incident(
                        "Desviación de Altitud",
                        $"Altitud reportada ({command.AltitudeMeters.Value:F1}m) difiere en más de {maxAltitudeDeviation}m respecto al plan ({mission.FlightParameters.AltitudeMeters:F1}m).",
                        timestamp,
                        missionId,
                        droneId,
                        IncidentSeverity.Medium);

                    await incidentRepository.AddAsync(altitudeIncident, cancellationToken);
                }
            }

            await unitOfWork.CompleteAsync(cancellationToken);

            operationStatus.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<OperationStatus>.Success(operationStatus);
        }
        catch (DomainException ex)
        {
            return Result<OperationStatus>.Failure(ex.Error);
        }
    }

    public async Task<Result<OperationStatus>> Handle(UpdateOperationStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var operationStatus = await operationStatusRepository.FindByIdAsync(command.Id, cancellationToken);
        if (operationStatus is null)
        {
            return Result<OperationStatus>.Failure(FlightOperationsErrors.OperationStatusNotFound(command.Id));
        }

        try
        {
            var location = GeoCoordinate.Create(command.Latitude, command.Longitude);
            operationStatus.UpdateLocation(location, command.Status);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<OperationStatus>.Success(operationStatus);
        }
        catch (DomainException ex)
        {
            return Result<OperationStatus>.Failure(ex.Error);
        }
    }

    public async Task<Result> Handle(DeleteOperationStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var operationStatus = await operationStatusRepository.FindByIdAsync(command.Id, cancellationToken);
        if (operationStatus is null)
        {
            return Result.Failure(FlightOperationsErrors.OperationStatusNotFound(command.Id));
        }

        operationStatusRepository.Remove(operationStatus);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
