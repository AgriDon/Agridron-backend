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

internal sealed class IncidentCommandService(
    IIncidentRepository incidentRepository,
    IMissionRepository missionRepository,
    IDroneRepository droneRepository,
    IUnitOfWork unitOfWork) : IIncidentCommandService
{
    public async Task<Result<Incident>> Handle(CreateIncidentCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        MissionId? missionId = null;
        if (command.MissionId.HasValue)
        {
            var mission = await missionRepository.FindByIdAsync(command.MissionId.Value, cancellationToken);
            if (mission is null || !mission.IsActive)
            {
                return Result<Incident>.Failure(FlightOperationsErrors.MissionNotFound(command.MissionId.Value));
            }

            missionId = new MissionId(command.MissionId.Value);
        }

        DroneId? droneId = null;
        if (command.DroneId.HasValue)
        {
            var drone = await droneRepository.FindByIdAsync(command.DroneId.Value, cancellationToken);
            if (drone is null || !drone.IsActive)
            {
                return Result<Incident>.Failure(FlightOperationsErrors.DroneNotFound(command.DroneId.Value));
            }

            droneId = new DroneId(command.DroneId.Value);
        }

        var severity = IncidentSeverity.Medium;
        if (!string.IsNullOrWhiteSpace(command.Severity) &&
            Enum.TryParse<IncidentSeverity>(command.Severity, true, out var parsedSeverity))
        {
            severity = parsedSeverity;
        }

        try
        {
            var incident = new Incident(
                command.Type,
                command.Description,
                command.OccurredAt ?? DateTimeOffset.UtcNow,
                missionId,
                droneId,
                severity);

            await incidentRepository.AddAsync(incident, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            incident.MarkAsCreated();
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Incident>.Success(incident);
        }
        catch (DomainException ex)
        {
            return Result<Incident>.Failure(ex.Error);
        }
    }

    public async Task<Result<Incident>> Handle(UpdateIncidentCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var incident = await incidentRepository.FindByIdAsync(command.Id, cancellationToken);
        if (incident is null)
        {
            return Result<Incident>.Failure(FlightOperationsErrors.IncidentNotFound(command.Id));
        }

        var severity = IncidentSeverity.Medium;
        if (!string.IsNullOrWhiteSpace(command.Severity) &&
            Enum.TryParse<IncidentSeverity>(command.Severity, true, out var parsedSeverity))
        {
            severity = parsedSeverity;
        }

        try
        {
            incident.Update(command.Type, command.Description, severity, command.OccurredAt);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Incident>.Success(incident);
        }
        catch (DomainException ex)
        {
            return Result<Incident>.Failure(ex.Error);
        }
    }

    public async Task<Result> Handle(DeleteIncidentCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var incident = await incidentRepository.FindByIdAsync(command.Id, cancellationToken);
        if (incident is null)
        {
            return Result.Failure(FlightOperationsErrors.IncidentNotFound(command.Id));
        }

        incidentRepository.Remove(incident);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
