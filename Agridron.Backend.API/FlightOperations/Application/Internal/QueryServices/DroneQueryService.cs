using Agridron.Backend.API.FlightOperations.Application.QueryServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices;

internal sealed class DroneQueryService(IDroneRepository droneRepository) : IDroneQueryService
{
    public async Task<Result<Drone>> Handle(GetDroneByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var drone = await droneRepository.FindByIdAsync(query.Id, cancellationToken);
        return drone is null || !drone.IsActive
            ? Result<Drone>.Failure(FlightOperationsErrors.DroneNotFound(query.Id))
            : Result<Drone>.Success(drone);
    }

    public async Task<Result<PagedResult<Drone>>> Handle(GetDronesFilteredQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        DroneStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var clean = query.Status.Trim().Replace("_", "");
            if (Enum.TryParse<DroneStatus>(clean, true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }
        }

        var drones = await droneRepository.ListFilteredAsync(
            statusFilter,
            query.MinCapacity,
            query.MaxCapacity,
            onlyActive: true,
            cancellationToken);

        return Result<PagedResult<Drone>>.Success(Paging.Apply(drones, query.Pagination));
    }
}
