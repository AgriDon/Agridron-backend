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
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Drone Endpoints.")]
public class DronesController(
    IDroneCommandService droneCommandService,
    IDroneQueryService droneQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all drones",
        Description = "Retrieves active drones, optionally filtered by status or tank capacity with pagination support.",
        OperationId = "GetDrones")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of drones retrieved successfully", typeof(IEnumerable<DroneResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetDronesAsync(
        [FromQuery] string? status,
        [FromQuery] decimal? minCapacity,
        [FromQuery] decimal? maxCapacity,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetDronesFilteredQuery(status, minCapacity, maxCapacity, pagination);
        var result = await droneQueryService.Handle(query, cancellationToken);

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

        return Ok(result.Value!.Items.Select(DroneAssembler.ToResourceFrom));
    }

    [HttpGet("{id:long}", Name = "GetDroneById")]
    [SwaggerOperation(
        Summary = "Get drone by ID",
        Description = "Retrieves operational details and telemetry summary of a specific drone by its ID.",
        OperationId = "GetDroneById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Drone found successfully", typeof(DroneResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Drone not found or inactive")]
    public async Task<IActionResult> GetDroneByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetDroneByIdQuery(id);
        var result = await droneQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(DroneAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Register drone",
        Description = "Registers a new agricultural drone in the fleet with tank capacity and technical specs.",
        OperationId = "CreateDrone")]
    [SwaggerResponse(StatusCodes.Status201Created, "Drone registered successfully", typeof(DroneResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid drone payload")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "A drone with the specified serial number already exists")]
    public async Task<IActionResult> CreateDroneAsync(
        [FromBody] CreateDroneResource resource,
        CancellationToken cancellationToken)
    {
        var command = DroneAssembler.ToCommandFrom(resource);
        var result = await droneCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var droneResource = DroneAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetDroneById", new { id = droneResource.Id }, droneResource);
    }

    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update drone",
        Description = "Updates the specifications, tank capacity, maintenance dates or image of an existing drone.",
        OperationId = "UpdateDrone")]
    [SwaggerResponse(StatusCodes.Status200OK, "Drone updated successfully", typeof(DroneResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Drone not found")]
    public async Task<IActionResult> UpdateDroneAsync(
        long id,
        [FromBody] UpdateDroneResource resource,
        CancellationToken cancellationToken)
    {
        var command = DroneAssembler.ToCommandFrom(id, resource);
        var result = await droneCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(DroneAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Deactivate drone",
        Description = "Inactivates a drone (soft delete) to retire it from active operations while maintaining historical mission logs.",
        OperationId = "DeleteDrone")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Drone deactivated successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Drone not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Drone is currently in flight or assigned to an active mission")]
    public async Task<IActionResult> DeleteDroneAsync(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteDroneCommand(id);
        var result = await droneCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }


        [HttpPost("{id:long}/maintenance")]
    [SwaggerOperation(
        Summary = "Send drone to maintenance ",
        Description = "Takes an AVAILABLE drone out of operation for a planned maintenance. The drone status changes to MAINTENANCE.",
        OperationId = "SendDroneToMaintenance")]
    [SwaggerResponse(StatusCodes.Status200OK, "Drone sent to maintenance", typeof(DroneResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Drone not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Drone is not available")]
    public async Task<IActionResult> SendDroneToMaintenanceAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var command = new SendDroneToMaintenanceCommand(id);
        var result = await droneCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(DroneAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/maintenance/complete")]
    [SwaggerOperation(
        Summary = "Complete drone maintenance or repair ",
        Description = "Saves the maintenance in the drone history (technician, type, description), updates its maintenance dates " +
                      "and returns a drone in MAINTENANCE or OUT_OF_SERVICE to AVAILABLE. A drone OUT_OF_SERVICE always gets a 'corrective' record.",
        OperationId = "CompleteDroneMaintenance")]
    [SwaggerResponse(StatusCodes.Status200OK, "Maintenance completed", typeof(DroneResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Drone not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Drone is not in maintenance")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Technician missing or invalid maintenance type")]
    public async Task<IActionResult> CompleteDroneMaintenanceAsync(
        long id,
        [FromBody] CompleteDroneMaintenanceResource resource,
        CancellationToken cancellationToken)
    {

        if (!MaintenanceRecordAssembler.TryParseType(resource.Type, out var type))
        {
            return problemDetailsFactory.CreateProblemDetails(this, FlightOperationsErrors.MaintenanceTypeInvalid);
        }

        var command = new CompleteDroneMaintenanceCommand(
            id,
            resource.MaintenanceDate,
            resource.NextMaintenanceDate,
            resource.Technician,
            type,
            resource.Description);

        var result = await droneCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(DroneAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/failure")]
    [SwaggerOperation(
        Summary = "Report drone failure ",
        Description = "Records a FALLA_TECNICA incident for the drone and sets its status to OUT_OF_SERVICE until it is repaired.",
        OperationId = "ReportDroneFailure")]
    [SwaggerResponse(StatusCodes.Status200OK, "Failure reported", typeof(DroneResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Failure description is required")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Drone not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Failure cannot be reported in the current status")]
    public async Task<IActionResult> ReportDroneFailureAsync(
        long id,
        [FromBody] ReportDroneFailureResource resource,
        CancellationToken cancellationToken)
    {
        var command = new ReportDroneFailureCommand(id, resource.Description, resource.OccurredAt);
        var result = await droneCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(DroneAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }


    [HttpGet("{id:long}/maintenance-records")]
    [SwaggerOperation(
        Summary = "Get drone maintenance history",
        Description = "Lists the maintenance and repair records of a drone, most recent first.",
        OperationId = "GetDroneMaintenanceRecords")]
    [SwaggerResponse(StatusCodes.Status200OK, "Maintenance history", typeof(IEnumerable<MaintenanceRecordResource>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Drone not found or inactive")]
    public async Task<IActionResult> GetDroneMaintenanceRecordsAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetMaintenanceRecordsByDroneIdQuery(id);
        var result = await droneQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value!.Select(MaintenanceRecordAssembler.ToResourceFrom))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

}
