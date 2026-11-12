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
internal sealed class ParcelRepository(AppDbContext context) : BaseRepository<Parcel>(context), IParcelRepository
{
    public async Task<IReadOnlyList<Parcel>> ListByFarmIdAsync(
        FarmId farmId,
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Parcel>().Where(p => p.FarmId == farmId);
        if (onlyActive)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByFarmIdAsync(
        FarmId farmId,
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Parcel>().Where(p => p.FarmId == farmId);
        if (onlyActive)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountByCropIdAsync(
        CropId cropId,
        bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Parcel>().Where(p => p.CropId == cropId);
        if (onlyActive)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.CountAsync(cancellationToken);
    }
}
