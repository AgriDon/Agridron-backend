using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;


public interface IChemicalRepository : IBaseRepository<Chemical>
{
    /// <summary>
    ///     Lists the active chemicals (soft-deleted ones are excluded), optionally filtered by type.
    /// </summary>
    Task<IReadOnlyList<Chemical>> ListActiveAsync(ChemicalType? type = null, CancellationToken cancellationToken = default);
}
