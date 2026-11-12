using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.QueryServices;

public interface IOperationStatusQueryService
{
    Task<Result<OperationStatus>> Handle(GetOperationStatusByIdQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OperationStatus>>> Handle(GetOperationStatusesFilteredQuery query, CancellationToken cancellationToken = default);
}
