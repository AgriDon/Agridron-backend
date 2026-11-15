using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class MaintenanceRecordRepository(AppDbContext context)
    : BaseRepository<MaintenanceRecord>(context), IMaintenanceRecordRepository
{
    public async Task<IReadOnlyList<MaintenanceRecord>> ListByDroneIdAsync(
        DroneId droneId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<MaintenanceRecord>()
            .Where(m => m.DroneId == droneId)
            .OrderByDescending(m => m.PerformedAt)
            .ThenByDescending(m => m.Id)
            .ToListAsync(cancellationToken);
    }
}
