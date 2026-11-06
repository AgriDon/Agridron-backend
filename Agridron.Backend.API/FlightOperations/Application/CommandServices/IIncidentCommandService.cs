using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.CommandServices;

public interface IIncidentCommandService
{
    Task<Result<Incident>> Handle(CreateIncidentCommand command, CancellationToken cancellationToken = default);
    Task<Result<Incident>> Handle(UpdateIncidentCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteIncidentCommand command, CancellationToken cancellationToken = default);
}
