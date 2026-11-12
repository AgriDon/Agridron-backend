using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FlightOperations.Domain.Repositories;

public interface IDroneRepository : IBaseRepository<Drone>
{
    Task<Drone?> FindBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithSerialNumberAsync(string serialNumber, DroneId? excludedId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Drone>> ListFilteredAsync(DroneStatus? status, decimal? minCapacity, decimal? maxCapacity, bool onlyActive = true, CancellationToken cancellationToken = default);
}
