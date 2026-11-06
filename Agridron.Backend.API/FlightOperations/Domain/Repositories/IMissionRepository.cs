using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FlightOperations.Domain.Repositories;

public interface IMissionRepository : IBaseRepository<Mission>
{
    Task<Mission?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithCodeAsync(string code, MissionId? excludedId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Mission>> ListFilteredAsync(
        long? farmId,
        long? parcelId,
        DroneId? droneId,
        MissionStatus? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);
}
