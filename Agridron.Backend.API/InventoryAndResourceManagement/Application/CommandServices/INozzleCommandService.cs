using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Application.CommandServices;

public interface INozzleCommandService
{
    Task<Result<Nozzle>> Handle(CreateNozzleCommand command, CancellationToken cancellationToken = default);
    Task<Result<Nozzle>> Handle(UpdateNozzleCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteNozzleCommand command, CancellationToken cancellationToken = default);
    Task<Result<Nozzle>> Handle(ToggleNozzleStatusCommand command, CancellationToken cancellationToken = default);
}
