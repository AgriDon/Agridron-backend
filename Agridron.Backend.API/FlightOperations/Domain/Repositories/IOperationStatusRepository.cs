using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FlightOperations.Domain.Repositories;

public interface IOperationStatusRepository : IBaseRepository<OperationStatus>
{
    Task<IReadOnlyList<OperationStatus>> ListFilteredAsync(
        MissionId? missionId,
        DroneId? droneId,
        CancellationToken cancellationToken = default);

    Task<OperationStatus?> GetLatestByMissionIdAsync(
        MissionId missionId,
        CancellationToken cancellationToken = default);
}
