using Agridron.Backend.API.FlightOperations.Application.QueryServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices;

internal sealed class MissionQueryService(IMissionRepository missionRepository) : IMissionQueryService
{
    public async Task<Result<Mission>> Handle(GetMissionByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var mission = await missionRepository.FindByIdAsync(query.Id, cancellationToken);
        return mission is null || !mission.IsActive
            ? Result<Mission>.Failure(FlightOperationsErrors.MissionNotFound(query.Id))
            : Result<Mission>.Success(mission);
    }

    public async Task<Result<PagedResult<Mission>>> Handle(GetMissionsFilteredQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        MissionStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var normalizedStatus = query.Status.Trim();
            if (Enum.TryParse<MissionStatus>(normalizedStatus, true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }
            else
            {
                // Spanish labels mapping commonly sent by frontend
                statusFilter = normalizedStatus.ToLowerInvariant() switch
                {
                    "programada" or "planificada" => MissionStatus.Planned,
                    "autorizada" => MissionStatus.Authorized,
                    "en curso" or "en progreso" => MissionStatus.InProgress,
                    "pausada" => MissionStatus.Paused,
                    "completada" => MissionStatus.Completed,
                    "cancelada" => MissionStatus.Cancelled,
                    _ => null
                };
            }
        }

        DroneId? droneIdFilter = query.DroneId.HasValue ? new DroneId(query.DroneId.Value) : null;

        var missions = await missionRepository.ListFilteredAsync(
            query.FarmId,
            query.ParcelId,
            droneIdFilter,
            statusFilter,
            query.From,
            query.To,
            cancellationToken);

        var activeMissions = missions.Where(m => m.IsActive).ToList();

        return Result<PagedResult<Mission>>.Success(Paging.Apply(activeMissions, query.Pagination));
    }
}
