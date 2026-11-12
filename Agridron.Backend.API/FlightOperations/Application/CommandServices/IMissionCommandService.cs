using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FlightOperations.Application.CommandServices;

public interface IMissionCommandService
{
    Task<Result<Mission>> Handle(CreateMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(UpdateMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(AssignMissionResourcesCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(AssignMissionOperatorCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(ConfigureFlightParametersCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(ApproveMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(StartMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(PauseMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(ResumeMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(CompleteMissionCommand command, CancellationToken cancellationToken = default);
    Task<Result<Mission>> Handle(CancelMissionCommand command, CancellationToken cancellationToken = default);
}
