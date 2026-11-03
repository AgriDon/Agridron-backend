using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Entity Framework Core implementation of <see cref="IFarmRepository" />.
/// </summary>
internal sealed class FarmRepository : BaseRepository<Farm>, IFarmRepository
{
    public FarmRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Farm>> ListByOwnerIdAsync(
        long ownerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Farm>()
            .Where(f => f.OwnerId == ownerId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(
        string name,
        long? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Farm>().Where(f => f.Name == name);

        if (excludedId.HasValue)
        {
            query = query.Where(f => f.Id.Value != excludedId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
