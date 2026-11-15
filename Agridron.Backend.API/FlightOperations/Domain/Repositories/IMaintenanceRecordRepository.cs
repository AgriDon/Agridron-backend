using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FlightOperations.Domain.Repositories;

public interface IMaintenanceRecordRepository : IBaseRepository<MaintenanceRecord>
{
    /// <summary>
    /// List the maintenance of a drone, from the first maintenance to last maintenance
    /// The most recent maintenance come first
    /// </summary>
    /// <param name="droneId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<MaintenanceRecord>> ListByDroneIdAsync(
        DroneId droneId,
        CancellationToken cancellationToken = default
        );



}
