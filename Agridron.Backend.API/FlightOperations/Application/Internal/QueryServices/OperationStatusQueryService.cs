using Agridron.Backend.API.FlightOperations.Application.QueryServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices;

internal sealed class OperationStatusQueryService(IOperationStatusRepository operationStatusRepository) : IOperationStatusQueryService
{
    public async Task<Result<OperationStatus>> Handle(GetOperationStatusByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var status = await operationStatusRepository.FindByIdAsync(query.Id, cancellationToken);
        return status is null
            ? Result<OperationStatus>.Failure(FlightOperationsErrors.OperationStatusNotFound(query.Id))
            : Result<OperationStatus>.Success(status);
    }

    public async Task<Result<PagedResult<OperationStatus>>> Handle(GetOperationStatusesFilteredQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        MissionId? missionId = query.MissionId.HasValue ? new MissionId(query.MissionId.Value) : null;
        DroneId? droneId = query.DroneId.HasValue ? new DroneId(query.DroneId.Value) : null;

        var statuses = await operationStatusRepository.ListFilteredAsync(missionId, droneId, cancellationToken);

        return Result<PagedResult<OperationStatus>>.Success(Paging.Apply(statuses, query.Pagination));
    }
}
