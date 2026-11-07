using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.CommandServices;

public interface IOperationStatusCommandService
{
    Task<Result<OperationStatus>> Handle(CreateOperationStatusCommand command, CancellationToken cancellationToken = default);
    Task<Result<OperationStatus>> Handle(UpdateOperationStatusCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteOperationStatusCommand command, CancellationToken cancellationToken = default);
}
