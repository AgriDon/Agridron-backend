using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Entity Framework Core implementation of <see cref="ICropRepository" />.
/// </summary>
internal sealed class CropRepository : BaseRepository<Crop>, ICropRepository
{
    public CropRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsWithNameAndVarietyAsync(
        string name,
        string variety,
        long? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Crop>()
            .Where(c => c.Name == name && c.Variety == variety);

        if (excludedId.HasValue)
        {
            var excludedCropId = new CropId(excludedId.Value);
            query = query.Where(c => c.Id != excludedCropId);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
