using Agridron.Backend.API.InventoryAndResourceManagement.Application.QueryServices;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Queries;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.Internal.QueryServices;

internal sealed class ChemicalQueryService(IChemicalRepository chemicalRepository) : IChemicalQueryService
{
    public async Task<Result<Chemical>> Handle(GetChemicalByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var chemical = await chemicalRepository.FindByIdAsync(query.Id, cancellationToken);
        return chemical is null || !chemical.IsActive
            ? Result<Chemical>.Failure(InventoryErrors.ChemicalNotFound(query.Id))
            : Result<Chemical>.Success(chemical);
    }

    public async Task<Result<IReadOnlyList<Chemical>>> Handle(GetAllChemicalsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var chemicals = await chemicalRepository.ListActiveAsync(query.Type, cancellationToken);
        return Result<IReadOnlyList<Chemical>>.Success(chemicals);
    }
}
