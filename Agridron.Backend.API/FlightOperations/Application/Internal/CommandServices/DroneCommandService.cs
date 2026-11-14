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

internal sealed class DroneCommandService(
    IDroneRepository droneRepository,
    IIncidentRepository incidentRepository,
    IUnitOfWork unitOfWork) : IDroneCommandService
{
    public async Task<Result<Drone>> Handle(CreateDroneCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var exists = await droneRepository.ExistsWithSerialNumberAsync(command.SerialNumber, cancellationToken: cancellationToken);
        if (exists)
        {
            return Result<Drone>.Failure(FlightOperationsErrors.DroneSerialNumberAlreadyExists(command.SerialNumber));
        }

        try
        {
            var drone = new Drone(
                command.SerialNumber,
                command.Model,
                command.CapacityLiters,
                command.NozzleId,
                command.ImageUrl);

            await droneRepository.AddAsync(drone, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            drone.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Drone>.Success(drone);
        }
        catch (DomainException ex)
        {
            return Result<Drone>.Failure(ex.Error);
        }
    }

    public async Task<Result<Drone>> Handle(UpdateDroneCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var drone = await droneRepository.FindByIdAsync(command.Id, cancellationToken);
        if (drone is null || !drone.IsActive)
        {
            return Result<Drone>.Failure(FlightOperationsErrors.DroneNotFound(command.Id));
        }

        try
        {
            drone.UpdateDetails(
                command.Model,
                command.CapacityLiters,
                command.NozzleId,
                command.ImageUrl,
                command.LastMaintenanceDate,
                command.NextMaintenanceDate);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Drone>.Success(drone);
        }
        catch (DomainException ex)
        {
            return Result<Drone>.Failure(ex.Error);
        }
    }

    public async Task<Result> Handle(DeleteDroneCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var drone = await droneRepository.FindByIdAsync(command.Id, cancellationToken);
        if (drone is null || !drone.IsActive)
        {
            return Result.Failure(FlightOperationsErrors.DroneNotFound(command.Id));
        }

        if (drone.Status is DroneStatus.InFlight or DroneStatus.Assigned)
        {
            return Result.Failure(FlightOperationsErrors.DroneBusy(drone.Id.Value, drone.Status.ToString()));
        }

        drone.Deactivate();
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

        public async Task<Result<Drone>> Handle(SendDroneToMaintenanceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var drone = await droneRepository.FindByIdAsync(command.Id, cancellationToken);
        if (drone is null || !drone.IsActive)
        {
            return Result<Drone>.Failure(FlightOperationsErrors.DroneNotFound(command.Id));
        }

        try
        {
            drone.SendToMaintenance();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Drone>.Success(drone);
        }
        catch (DomainException ex)
        {
            return Result<Drone>.Failure(ex.Error);
        }
    }

    public async Task<Result<Drone>> Handle(CompleteDroneMaintenanceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var drone = await droneRepository.FindByIdAsync(command.Id, cancellationToken);
        if (drone is null || !drone.IsActive)
        {
            return Result<Drone>.Failure(FlightOperationsErrors.DroneNotFound(command.Id));
        }

        try
        {
            drone.CompleteMaintenance(command.MaintenanceDate, command.NextMaintenanceDate);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Drone>.Success(drone);
        }
        catch (DomainException ex)
        {
            return Result<Drone>.Failure(ex.Error);
        }
    }

    public async Task<Result<Drone>> Handle(ReportDroneFailureCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            return Result<Drone>.Failure(FlightOperationsErrors.DroneFailureDescriptionRequired);
        }

        var drone = await droneRepository.FindByIdAsync(command.Id, cancellationToken);
        if (drone is null || !drone.IsActive)
        {
            return Result<Drone>.Failure(FlightOperationsErrors.DroneNotFound(command.Id));
        }

        try
        {
            drone.ReportFailure();

            var incident = new Incident(
                IncidentTypes.EquipmentFailure,
                command.Description.Trim(),
                command.OccurredAt ?? DateTimeOffset.UtcNow,
                missionId: null,
                droneId: drone.Id,
                severity: IncidentSeverity.High);

            await incidentRepository.AddAsync(incident, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            incident.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Drone>.Success(drone);
        }
        catch (DomainException ex)
        {
            return Result<Drone>.Failure(ex.Error);
        }
    }
}
