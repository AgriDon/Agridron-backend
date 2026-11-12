using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Queries;

public sealed record GetOperationStatusByIdQuery(long Id);

public sealed record GetOperationStatusesFilteredQuery(
    long? MissionId,
    long? DroneId,
    PaginationParameters Pagination);
