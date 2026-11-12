using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Entity Framework Core implementation of <see cref="IFumigationAreaRepository" />.
/// </summary>
internal sealed class FumigationAreaRepository : BaseRepository<FumigationArea>, IFumigationAreaRepository
{
    public FumigationAreaRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<FumigationArea>> ListByParcelIdAsync(
        ParcelId parcelId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FumigationArea>()
            .Where(f => f.ParcelId == parcelId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByParcelIdAsync(
        ParcelId parcelId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FumigationArea>()
            .CountAsync(f => f.ParcelId == parcelId, cancellationToken);
    }
}
