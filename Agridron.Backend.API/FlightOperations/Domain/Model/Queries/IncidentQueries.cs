using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Queries;

public sealed record GetIncidentByIdQuery(long Id);

public sealed record GetIncidentsFilteredQuery(
    long? MissionId,
    long? DroneId,
    string? Type,
    string? Severity,
    PaginationParameters Pagination);
