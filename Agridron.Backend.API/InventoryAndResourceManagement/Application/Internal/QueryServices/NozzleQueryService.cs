using Agridron.Backend.API.InventoryAndResourceManagement.Application.QueryServices;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Queries;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.Internal.QueryServices;

internal sealed class NozzleQueryService(INozzleRepository nozzleRepository) : INozzleQueryService
{
    public async Task<Result<Nozzle>> Handle(GetNozzleByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var nozzle = await nozzleRepository.FindByIdAsync(query.Id, cancellationToken);
        return nozzle is null || !nozzle.IsActive
            ? Result<Nozzle>.Failure(InventoryErrors.NozzleNotFound(query.Id))
            : Result<Nozzle>.Success(nozzle);
    }

    public async Task<Result<IReadOnlyList<Nozzle>>> Handle(GetAllNozzlesQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var nozzles = await nozzleRepository.ListActiveAsync(query.Status, cancellationToken);
        return Result<IReadOnlyList<Nozzle>>.Success(nozzles);
    }
}
