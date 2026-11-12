using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Queries;

public sealed record GetMissionByIdQuery(long Id);

public sealed record GetMissionsFilteredQuery(
    long? FarmId,
    long? ParcelId,
    long? DroneId,
    string? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    PaginationParameters Pagination);
