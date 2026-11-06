using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.QueryServices;

public interface IMissionQueryService
{
    Task<Result<Mission>> Handle(GetMissionByIdQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<Mission>>> Handle(GetMissionsFilteredQuery query, CancellationToken cancellationToken = default);
}
