using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class DroneRepository(AppDbContext context) : BaseRepository<Drone>(context), IDroneRepository
{
    public async Task<Drone?> FindBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Drone>()
            .FirstOrDefaultAsync(d => d.SerialNumber == serialNumber, cancellationToken);
    }

    public async Task<bool> ExistsWithSerialNumberAsync(string serialNumber, DroneId? excludedId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Drone>().Where(d => d.SerialNumber == serialNumber);
        if (excludedId.HasValue)
        {
            query = query.Where(d => d.Id != excludedId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Drone>> ListFilteredAsync(
        DroneStatus? status,
        decimal? minCapacity,
        decimal? maxCapacity,
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Drone>().AsQueryable();

        if (onlyActive)
        {
            query = query.Where(d => d.IsActive);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (minCapacity.HasValue)
        {
            query = query.Where(d => d.CapacityLiters >= minCapacity.Value);
        }

        if (maxCapacity.HasValue)
        {
            query = query.Where(d => d.CapacityLiters <= maxCapacity.Value);
        }

        return await query
            .OrderBy(d => d.Model)
            .ToListAsync(cancellationToken);
    }
}
