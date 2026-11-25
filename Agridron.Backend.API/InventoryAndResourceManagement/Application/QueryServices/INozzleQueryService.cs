using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.QueryServices;

public interface INozzleQueryService
{
    Task<Result<Nozzle>> Handle(GetNozzleByIdQuery query, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Nozzle>>> Handle(GetAllNozzlesQuery query, CancellationToken cancellationToken = default);
}
