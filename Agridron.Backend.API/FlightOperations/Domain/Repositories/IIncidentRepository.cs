using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.FlightOperations.Domain.Repositories;

public interface IIncidentRepository : IBaseRepository<Incident>
{
    Task<IReadOnlyList<Incident>> ListFilteredAsync(
        MissionId? missionId,
        DroneId? droneId,
        string? type,
        IncidentSeverity? severity,
        CancellationToken cancellationToken = default);
}
