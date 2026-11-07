using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class IncidentRepository(AppDbContext context) : BaseRepository<Incident>(context), IIncidentRepository
{
    public async Task<IReadOnlyList<Incident>> ListFilteredAsync(
        MissionId? missionId,
        DroneId? droneId,
        string? type,
        IncidentSeverity? severity,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Incident>().AsQueryable();

        if (missionId.HasValue)
        {
            query = query.Where(i => i.MissionId == missionId.Value);
        }

        if (droneId.HasValue)
        {
            query = query.Where(i => i.DroneId == droneId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(i => i.Type.Contains(type));
        }

        if (severity.HasValue)
        {
            query = query.Where(i => i.Severity == severity.Value);
        }

        return await query
            .OrderByDescending(i => i.OccurredAt)
            .ToListAsync(cancellationToken);
    }
}
