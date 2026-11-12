using System.Globalization;
using Agridron.Backend.API.FlightOperations.Application.CommandServices;
using Agridron.Backend.API.FlightOperations.Application.QueryServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Domain.Model.Queries;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Transform;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Flight Mission Endpoints.")]
public class MissionsController(
    IMissionCommandService missionCommandService,
    IMissionQueryService missionQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all flight missions",
        Description = "Retrieves agricultural flight missions with filtering by farm, parcel, drone, status, or date range.",
        OperationId = "GetMissions")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of missions retrieved successfully", typeof(IEnumerable<MissionResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetMissionsAsync(
        [FromQuery] long? farmId,
        [FromQuery] long? parcelId,
        [FromQuery] long? droneId,
        [FromQuery] string? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetMissionsFilteredQuery(farmId, parcelId, droneId, status, from, to, pagination);
        var result = await missionQueryService.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        if (pagination.IsPaged)
        {
            Response.Headers.Append(
                "X-Total-Count",
                result.Value!.TotalCount.ToString(CultureInfo.InvariantCulture));
        }

        return Ok(result.Value!.Items.Select(MissionAssembler.ToResourceFrom));
    }

    [HttpGet("{id:long}", Name = "GetMissionById")]
    [SwaggerOperation(
        Summary = "Get mission by ID",
        Description = "Retrieves operational details, flight parameters, and progress of a specific mission by its ID.",
        OperationId = "GetMissionById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission found successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> GetMissionByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetMissionByIdQuery(id);
        var result = await missionQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create flight mission",
        Description = "Plans a new flight mission for crop spraying or inspection, resolving parcel boundary snapshots via ACL.",
        OperationId = "CreateMission")]
    [SwaggerResponse(StatusCodes.Status201Created, "Mission planned successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid mission payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Associated farm or parcel not found")]
    public async Task<IActionResult> CreateMissionAsync(
        [FromBody] CreateMissionResource resource,
        CancellationToken cancellationToken)
    {
        var command = MissionAssembler.ToCommandFrom(resource);
        var result = await missionCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var missionResource = MissionAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetMissionById", new { id = missionResource.Id }, missionResource);
    }

    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update flight mission",
        Description = "Updates general attributes, scheduled date or notes of an existing planned mission.",
        OperationId = "UpdateMission")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission updated successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission, farm, or parcel not found")]
    public async Task<IActionResult> UpdateMissionAsync(
        long id,
        [FromBody] UpdateMissionResource resource,
        CancellationToken cancellationToken)
    {
        var command = MissionAssembler.ToCommandFrom(id, resource);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Cancel or delete flight mission",
        Description = "Inactivates a flight mission and frees allocated drone resources.",
        OperationId = "DeleteMission")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Mission deactivated successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Mission is currently in progress and cannot be deleted")]
    public async Task<IActionResult> DeleteMissionAsync(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteMissionCommand(id);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/resources")]
    [SwaggerOperation(
        Summary = "Assign mission resources (MI-002)",
        Description = "Assigns an available drone, chemical product, and application dose to the mission.",
        OperationId = "AssignMissionResources")]
    [SwaggerResponse(StatusCodes.Status200OK, "Resources assigned successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid resource payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission or drone not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Drone is not available")]
    public async Task<IActionResult> AssignResourcesAsync(
        long id,
        [FromBody] AssignMissionResourcesResource resource,
        CancellationToken cancellationToken)
    {
        var command = MissionAssembler.ToCommandFrom(id, resource);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/operator")]
    [SwaggerOperation(
        Summary = "Assign mission operator (MI-003)",
        Description = "Designates the certified technical pilot or operator in charge of mission supervision.",
        OperationId = "AssignMissionOperator")]
    [SwaggerResponse(StatusCodes.Status200OK, "Operator assigned successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid operator payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> AssignOperatorAsync(
        long id,
        [FromBody] AssignMissionOperatorResource resource,
        CancellationToken cancellationToken)
    {
        var command = MissionAssembler.ToCommandFrom(id, resource);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/flight-parameters")]
    [SwaggerOperation(
        Summary = "Configure flight parameters (MI-004)",
        Description = "Configures autonomous flight altitude (1-30m), speed (1-20m/s), spray rate (1-100L/ha), and swath overlap (0-80%).",
        OperationId = "ConfigureFlightParameters")]
    [SwaggerResponse(StatusCodes.Status200OK, "Parameters configured successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parameter values out of safe operational envelope")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> ConfigureFlightParametersAsync(
        long id,
        [FromBody] ConfigureFlightParametersResource resource,
        CancellationToken cancellationToken)
    {
        var command = MissionAssembler.ToCommandFrom(id, resource);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/approve")]
    [SwaggerOperation(
        Summary = "Approve mission for deployment (MI-007)",
        Description = "Formally validates operational readiness and authorizes the mission for takeoff.",
        OperationId = "ApproveMission")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission authorized successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Mission missing required resources for flight")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> ApproveMissionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var command = new ApproveMissionCommand(id);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/start")]
    [SwaggerOperation(
        Summary = "Start mission execution (TR-001)",
        Description = "Launches the flight mission, transitioning drone status to IN_FLIGHT.",
        OperationId = "StartMission")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission started successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Mission cannot be started in current state")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> StartMissionAsync(
        long id,
        [FromBody] StartMissionResource? resource,
        CancellationToken cancellationToken)
    {
        var command = new StartMissionCommand(id, resource?.StartedAt);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/pause")]
    [SwaggerOperation(
        Summary = "Pause mission execution (TR-004)",
        Description = "Temporarily suspends flight execution, saving hover coordinates and setting drone status to PAUSED.",
        OperationId = "PauseMission")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission paused successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Mission is not currently in progress")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> PauseMissionAsync(
        long id,
        [FromBody] PauseMissionResource? resource,
        CancellationToken cancellationToken)
    {
        var command = new PauseMissionCommand(id, resource?.Latitude, resource?.Longitude);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/resume")]
    [SwaggerOperation(
        Summary = "Resume paused mission (TR-004)",
        Description = "Resumes a paused flight mission from its last coordinates.",
        OperationId = "ResumeMission")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission resumed successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Mission is not currently paused")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> ResumeMissionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var command = new ResumeMissionCommand(id);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/complete")]
    [SwaggerOperation(
        Summary = "Complete mission application (TR-005)",
        Description = "Marks flight mission as completed, registers treated surface area, and releases drone to AVAILABLE status.",
        OperationId = "CompleteMission")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission completed successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Mission cannot be completed")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> CompleteMissionAsync(
        long id,
        [FromBody] CompleteMissionResource? resource,
        CancellationToken cancellationToken)
    {
        var command = new CompleteMissionCommand(
            id,
            resource?.CompletedAt,
            resource?.TreatedAreaHectares,
            resource?.ClosingNotes);

        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/cancel")]
    [SwaggerOperation(
        Summary = "Cancel flight mission (MI-006)",
        Description = "Cancels a scheduled or in-progress mission due to weather or operational changes, releasing allocated drone.",
        OperationId = "CancelMission")]
    [SwaggerResponse(StatusCodes.Status200OK, "Mission cancelled successfully", typeof(MissionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Completed mission cannot be cancelled")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Mission not found")]
    public async Task<IActionResult> CancelMissionAsync(
        long id,
        [FromBody] CancelMissionResource? resource,
        CancellationToken cancellationToken)
    {
        var command = new CancelMissionCommand(id, resource?.Reason);
        var result = await missionCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(MissionAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
