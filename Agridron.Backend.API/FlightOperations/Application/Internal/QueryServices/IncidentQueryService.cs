using Agridron.Backend.API.FlightOperations.Application.QueryServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices;

internal sealed class IncidentQueryService(IIncidentRepository incidentRepository) : IIncidentQueryService
{
    public async Task<Result<Incident>> Handle(GetIncidentByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var incident = await incidentRepository.FindByIdAsync(query.Id, cancellationToken);
        return incident is null
            ? Result<Incident>.Failure(FlightOperationsErrors.IncidentNotFound(query.Id))
            : Result<Incident>.Success(incident);
    }

    public async Task<Result<PagedResult<Incident>>> Handle(GetIncidentsFilteredQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        MissionId? missionId = query.MissionId.HasValue ? new MissionId(query.MissionId.Value) : null;
        DroneId? droneId = query.DroneId.HasValue ? new DroneId(query.DroneId.Value) : null;

        IncidentSeverity? severity = null;
        if (!string.IsNullOrWhiteSpace(query.Severity))
        {
            var cleanSeverity = query.Severity.Trim().Replace("_", "");
            if (Enum.TryParse<IncidentSeverity>(cleanSeverity, true, out var parsedSeverity))
            {
                severity = parsedSeverity;
            }
        }

        var incidents = await incidentRepository.ListFilteredAsync(
            missionId,
            droneId,
            query.Type,
            severity,
            cancellationToken);

        return Result<PagedResult<Incident>>.Success(Paging.Apply(incidents, query.Pagination));
    }
}
