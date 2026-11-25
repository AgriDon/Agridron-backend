using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.CommandServices;

public interface IChemicalCommandService
{
    Task<Result<Chemical>> Handle(CreateChemicalCommand command, CancellationToken cancellationToken = default);
    Task<Result<Chemical>> Handle(UpdateChemicalCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteChemicalCommand command, CancellationToken cancellationToken = default);
    Task<Result<Chemical>> Handle(RestockChemicalCommand command, CancellationToken cancellationToken = default);
}
