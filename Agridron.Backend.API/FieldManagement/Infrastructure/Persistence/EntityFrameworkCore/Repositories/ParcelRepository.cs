using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Entity Framework Core implementation of <see cref="IParcelRepository" />.
/// </summary>
internal sealed class ParcelRepository : BaseRepository<Parcel>, IParcelRepository
{
    public ParcelRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Parcel>> ListByFarmIdAsync(
        FarmId farmId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Parcel>()
            .Where(p => p.FarmId == farmId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByFarmIdAsync(
        FarmId farmId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Parcel>()
            .CountAsync(p => p.FarmId == farmId, cancellationToken);
    }

    public async Task<int> CountByCropIdAsync(
        CropId cropId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Parcel>()
            .CountAsync(p => p.CropId == cropId, cancellationToken);
    }
}
