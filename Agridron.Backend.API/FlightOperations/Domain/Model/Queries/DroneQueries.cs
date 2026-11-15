using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Queries;

public sealed record GetDroneByIdQuery(long Id);

public sealed record GetDronesFilteredQuery(
    string? Status,
    decimal? MinCapacity,
    decimal? MaxCapacity,
    PaginationParameters Pagination);

/// <summary>
/// Maintenance history of a drone, most recent first
/// </summary>
/// <param name="DroneId"></param>
public sealed record GetMaintenanceRecordsByDroneIdQuery(long DroneId);
