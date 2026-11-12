using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.CommandServices;

public interface IDroneCommandService
{
    Task<Result<Drone>> Handle(CreateDroneCommand command, CancellationToken cancellationToken = default);
    Task<Result<Drone>> Handle(UpdateDroneCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteDroneCommand command, CancellationToken cancellationToken = default);
}
