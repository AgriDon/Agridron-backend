using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class MissionRepository(AppDbContext context) : BaseRepository<Mission>(context), IMissionRepository
{
    public async Task<Mission?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Mission>()
            .FirstOrDefaultAsync(m => m.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsWithCodeAsync(string code, MissionId? excludedId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Mission>().Where(m => m.Code == code);
        if (excludedId.HasValue)
        {
            query = query.Where(m => m.Id != excludedId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Mission>> ListFilteredAsync(
        long? farmId,
        long? parcelId,
        DroneId? droneId,
        MissionStatus? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Mission>().AsQueryable();

        if (farmId.HasValue)
        {
            query = query.Where(m => m.FarmId == farmId.Value);
        }

        if (parcelId.HasValue)
        {
            query = query.Where(m => m.ParcelId == parcelId.Value);
        }

        if (droneId.HasValue)
        {
            query = query.Where(m => m.DroneId == droneId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(m => m.ScheduledAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(m => m.ScheduledAt <= to.Value);
        }

        return await query
            .OrderByDescending(m => m.ScheduledAt)
            .ToListAsync(cancellationToken);
    }
}
