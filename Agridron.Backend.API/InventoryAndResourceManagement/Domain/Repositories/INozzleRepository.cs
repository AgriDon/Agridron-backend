using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;


public interface INozzleRepository : IBaseRepository<Nozzle>
{
    /// <summary>
    ///     Lists the active nozzles (soft-deleted ones are excluded), optionally filtered by status.
    /// </summary>
    Task<IReadOnlyList<Nozzle>> ListActiveAsync(NozzleStatus? status = null, CancellationToken cancellationToken = default);
}
