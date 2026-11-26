using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class ChemicalRepository(AppDbContext context) : BaseRepository<Chemical>(context), IChemicalRepository
{
    public async Task<IReadOnlyList<Chemical>> ListActiveAsync(
        ChemicalType? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Chemical>().Where(c => c.IsActive);

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        return await query
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}
