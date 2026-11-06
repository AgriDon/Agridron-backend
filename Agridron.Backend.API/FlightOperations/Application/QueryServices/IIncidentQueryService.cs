using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.QueryServices;

public interface IIncidentQueryService
{
    Task<Result<Incident>> Handle(GetIncidentByIdQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<Incident>>> Handle(GetIncidentsFilteredQuery query, CancellationToken cancellationToken = default);
}
