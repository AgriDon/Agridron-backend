using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.QueryServices;

public interface IChemicalQueryService
{
    Task<Result<Chemical>> Handle(GetChemicalByIdQuery query, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Chemical>>> Handle(GetAllChemicalsQuery query, CancellationToken cancellationToken = default);

}
