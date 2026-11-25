using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class NozzleRepository(AppDbContext context) : BaseRepository<Nozzle>(context), INozzleRepository
{
    public async Task<IReadOnlyList<Nozzle>> ListActiveAsync(
        NozzleStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Nozzle>().Where(n => n.IsActive);

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        return await query
            .OrderBy(n => n.Id)
            .ToListAsync(cancellationToken);
    }
}
