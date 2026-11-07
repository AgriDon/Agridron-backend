using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class OperationStatusRepository(AppDbContext context) : BaseRepository<OperationStatus>(context), IOperationStatusRepository
{
    public async Task<IReadOnlyList<OperationStatus>> ListFilteredAsync(
        MissionId? missionId,
        DroneId? droneId,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<OperationStatus>().AsQueryable();

        if (missionId.HasValue)
        {
            query = query.Where(o => o.MissionId == missionId.Value);
        }

        if (droneId.HasValue)
        {
            query = query.Where(o => o.DroneId == droneId.Value);
        }

        return await query
            .OrderByDescending(o => o.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationStatus?> GetLatestByMissionIdAsync(
        MissionId missionId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<OperationStatus>()
            .Where(o => o.MissionId == missionId)
            .OrderByDescending(o => o.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
