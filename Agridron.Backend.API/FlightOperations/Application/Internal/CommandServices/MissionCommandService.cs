using Agridron.Backend.API.FlightOperations.Application.CommandServices;
using Agridron.Backend.API.FlightOperations.Application.Internal.OutboundServices.ACL;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices;

internal sealed class MissionCommandService(
    IMissionRepository missionRepository,
    IDroneRepository droneRepository,
    IExternalFieldService fieldService,
    IUnitOfWork unitOfWork) : IMissionCommandService
{
    public async Task<Result<Mission>> Handle(CreateMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var code = string.IsNullOrWhiteSpace(command.Code)
            ? $"MS-{DateTimeOffset.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
            : command.Code.Trim();

        var exists = await missionRepository.ExistsWithCodeAsync(code, cancellationToken: cancellationToken);
        if (exists)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionCodeAlreadyExists(code));
        }

        if (command.FarmId.HasValue)
        {
            var farmExists = await fieldService.ExistsFarmAsync(command.FarmId.Value, cancellationToken);
            if (!farmExists)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.FarmNotFound(command.FarmId.Value));
            }
        }

        string farmArea = command.FarmArea?.Trim() ?? "Parcela Principal";
        string cropType = command.CropType?.Trim() ?? "General";
        decimal plannedAreaHectares = command.PlannedAreaHectares ?? 1.0m;

        if (command.ParcelId.HasValue)
        {
            var snapshot = await fieldService.FetchParcelSnapshotAsync(command.ParcelId.Value, cancellationToken);
            if (snapshot is null)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.ParcelNotFound(command.ParcelId.Value));
            }

            if (command.FarmId.HasValue && snapshot.FarmId != command.FarmId.Value)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.ParcelDoesNotBelongToFarm(command.ParcelId.Value, command.FarmId.Value));
            }

            if (string.IsNullOrWhiteSpace(command.FarmArea))
            {
                farmArea = snapshot.Name;
            }

            if (string.IsNullOrWhiteSpace(command.CropType) && !string.IsNullOrWhiteSpace(snapshot.CropName))
            {
                cropType = snapshot.CropName;
            }

            if (!command.PlannedAreaHectares.HasValue || command.PlannedAreaHectares.Value <= 0m)
            {
                plannedAreaHectares = snapshot.AreaHectares > 0m ? snapshot.AreaHectares : 1.0m;
            }
        }

        Drone? drone = null;
        if (command.DroneId.HasValue)
        {
            drone = await droneRepository.FindByIdAsync(command.DroneId.Value, cancellationToken);
            if (drone is null || !drone.IsActive)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.DroneNotFound(command.DroneId.Value));
            }

            if (drone.Status != DroneStatus.Available)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.DroneNotAvailable(drone.Id.Value, drone.Status.ToString()));
            }
        }

        try
        {
            var scheduledAt = command.ScheduledAt ?? DateTimeOffset.UtcNow;
            var droneIdVo = command.DroneId.HasValue ? new DroneId(command.DroneId.Value) : (DroneId?)null;

            var mission = new Mission(
                code,
                command.FarmId,
                command.ParcelId,
                farmArea,
                cropType,
                scheduledAt,
                plannedAreaHectares,
                command.OperatorName,
                command.TreatmentType,
                command.ProductName,
                command.ProductDose,
                command.Notes,
                droneIdVo,
                command.Progress ?? 0);

            if (drone is not null)
            {
                drone.AssignToMission();
            }

            await missionRepository.AddAsync(mission, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            mission.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(UpdateMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.Id, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.Id));
        }

        if (command.FarmId.HasValue)
        {
            var farmExists = await fieldService.ExistsFarmAsync(command.FarmId.Value, cancellationToken);
            if (!farmExists)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.FarmNotFound(command.FarmId.Value));
            }
        }

        if (command.ParcelId.HasValue)
        {
            var snapshot = await fieldService.FetchParcelSnapshotAsync(command.ParcelId.Value, cancellationToken);
            if (snapshot is null)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.ParcelNotFound(command.ParcelId.Value));
            }

            if (command.FarmId.HasValue && snapshot.FarmId != command.FarmId.Value)
            {
                return Result<Mission>.Failure(FlightOperationsErrors.ParcelDoesNotBelongToFarm(command.ParcelId.Value, command.FarmId.Value));
            }
        }

        try
        {
            mission.UpdateGeneralDetails(
                command.FarmId ?? mission.FarmId,
                command.ParcelId ?? mission.ParcelId,
                command.FarmArea ?? mission.FarmArea,
                command.CropType ?? mission.CropType,
                command.ScheduledAt ?? mission.ScheduledAt,
                command.PlannedAreaHectares ?? mission.PlannedAreaHectares,
                command.TreatmentType ?? mission.TreatmentType,
                command.ProductName ?? mission.ProductName,
                command.ProductDose ?? mission.ProductDose,
                command.Notes ?? mission.Notes,
                command.OperatorName ?? mission.OperatorName);

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result> Handle(DeleteMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.Id, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result.Failure(FlightOperationsErrors.MissionNotFound(command.Id));
        }

        if (mission.Status == MissionStatus.InProgress)
        {
            return Result.Failure(FlightOperationsErrors.MissionInProgress(mission.Id.Value));
        }

        if (mission.DroneId.HasValue && mission.Status != MissionStatus.Completed && mission.Status != MissionStatus.Cancelled)
        {
            var drone = await droneRepository.FindByIdAsync(mission.DroneId.Value.Value, cancellationToken);
            if (drone is not null && drone.Status is DroneStatus.Assigned or DroneStatus.Paused)
            {
                drone.ReleaseFromMission();
            }
        }

        mission.Deactivate();
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Mission>> Handle(AssignMissionResourcesCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        var drone = await droneRepository.FindByIdAsync(command.DroneId, cancellationToken);
        if (drone is null || !drone.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.DroneNotFound(command.DroneId));
        }

        if (drone.Status != DroneStatus.Available && mission.DroneId?.Value != command.DroneId)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.DroneNotAvailable(drone.Id.Value, drone.Status.ToString()));
        }

        try
        {
            if (mission.DroneId.HasValue && mission.DroneId.Value.Value != command.DroneId)
            {
                var prevDrone = await droneRepository.FindByIdAsync(mission.DroneId.Value.Value, cancellationToken);
                if (prevDrone is not null && prevDrone.Status is DroneStatus.Assigned or DroneStatus.Paused)
                {
                    prevDrone.ReleaseFromMission();
                }
            }

            mission.AssignResources(
                new DroneId(command.DroneId),
                command.TreatmentType,
                command.ProductName,
                command.ProductDose,
                command.ChemicalId);

            drone.AssignToMission();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(AssignMissionOperatorCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            mission.AssignOperator(command.OperatorName, command.OperatorId);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(ConfigureFlightParametersCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            var parameters = FlightParameters.Create(
                command.AltitudeMeters,
                command.SpeedMetersPerSecond,
                command.SprayRateLitersPerHectare,
                command.OverlapPercentage);

            mission.ConfigureFlightParameters(parameters);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(ApproveMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            mission.Approve();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(StartMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            var startedAt = command.StartedAt ?? DateTimeOffset.UtcNow;
            mission.Start(startedAt);

            if (mission.DroneId.HasValue)
            {
                var drone = await droneRepository.FindByIdAsync(mission.DroneId.Value.Value, cancellationToken);
                drone?.MarkInFlight();
            }

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(PauseMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            GeoCoordinate? pausePoint = null;
            if (command.Latitude.HasValue && command.Longitude.HasValue)
            {
                pausePoint = GeoCoordinate.Create(command.Latitude.Value, command.Longitude.Value);
            }

            mission.Pause(pausePoint);

            if (mission.DroneId.HasValue)
            {
                var drone = await droneRepository.FindByIdAsync(mission.DroneId.Value.Value, cancellationToken);
                drone?.MarkPaused();
            }

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(ResumeMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            mission.Resume();

            if (mission.DroneId.HasValue)
            {
                var drone = await droneRepository.FindByIdAsync(mission.DroneId.Value.Value, cancellationToken);
                drone?.MarkInFlight();
            }

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(CompleteMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            var completedAt = command.CompletedAt ?? DateTimeOffset.UtcNow;
            mission.Complete(completedAt, command.TreatedAreaHectares, command.ClosingNotes);

            if (mission.DroneId.HasValue)
            {
                var drone = await droneRepository.FindByIdAsync(mission.DroneId.Value.Value, cancellationToken);
                if (drone is not null)
                {
                    var flightHours = (mission.CompletedAt - mission.StartedAt)?.TotalHours ?? 0;
                    drone.ReleaseFromMission((decimal)Math.Max(0, flightHours));
                }
            }

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }

    public async Task<Result<Mission>> Handle(CancelMissionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var mission = await missionRepository.FindByIdAsync(command.MissionId, cancellationToken);
        if (mission is null || !mission.IsActive)
        {
            return Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId));
        }

        try
        {
            mission.Cancel(command.Reason);

            if (mission.DroneId.HasValue)
            {
                var drone = await droneRepository.FindByIdAsync(mission.DroneId.Value.Value, cancellationToken);
                if (drone is not null && drone.Status is DroneStatus.Assigned or DroneStatus.InFlight or DroneStatus.Paused)
                {
                    drone.ReleaseFromMission();
                }
            }

            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Mission>.Success(mission);
        }
        catch (DomainException ex)
        {
            return Result<Mission>.Failure(ex.Error);
        }
    }
}
