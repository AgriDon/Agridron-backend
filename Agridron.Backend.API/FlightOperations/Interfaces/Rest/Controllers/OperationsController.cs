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
[SwaggerTag("Available Flight Operation Telemetry Endpoints.")]
public class OperationsController(
    IOperationStatusCommandService operationStatusCommandService,
    IOperationStatusQueryService operationStatusQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get flight operation telemetry points",
        Description = "Retrieves real-time and historical drone telemetry points (GPS, altitude, speed, battery, tank volume).",
        OperationId = "GetOperations")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of operation telemetry points retrieved successfully", typeof(IEnumerable<OperationStatusResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetOperationsAsync(
        [FromQuery] long? missionId,
        [FromQuery] long? droneId,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetOperationStatusesFilteredQuery(missionId, droneId, pagination);
        var result = await operationStatusQueryService.Handle(query, cancellationToken);

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

        return Ok(result.Value!.Items.Select(OperationStatusAssembler.ToResourceFrom));
    }

    [HttpGet("{id:long}", Name = "GetOperationById")]
    [SwaggerOperation(
        Summary = "Get operation telemetry point by ID",
        Description = "Retrieves a specific GPS coordinate and operational telemetry record by its ID.",
        OperationId = "GetOperationById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Operation status found successfully", typeof(OperationStatusResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Operation status not found")]
    public async Task<IActionResult> GetOperationByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetOperationStatusByIdQuery(id);
        var result = await operationStatusQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(OperationStatusAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Record flight telemetry point (TR-001, TR-002, TR-003)",
        Description = "Records a new telemetry position reading, synchronizes drone battery state, and triggers anomaly alerts if values are out-of-bounds.",
        OperationId = "CreateOperation")]
    [SwaggerResponse(StatusCodes.Status201Created, "Telemetry point recorded successfully", typeof(OperationStatusResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid telemetry payload or coordinate out of range")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Associated mission not found")]
    public async Task<IActionResult> CreateOperationAsync(
        [FromBody] CreateOperationStatusResource resource,
        CancellationToken cancellationToken)
    {
        var command = OperationStatusAssembler.ToCommandFrom(resource);
        var result = await operationStatusCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var operationResource = OperationStatusAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetOperationById", new { id = operationResource.Id }, operationResource);
    }

    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update operation status",
        Description = "Updates the coordinates or state label of an operation telemetry record.",
        OperationId = "UpdateOperation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Operation updated successfully", typeof(OperationStatusResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Operation status not found")]
    public async Task<IActionResult> UpdateOperationAsync(
        long id,
        [FromBody] UpdateOperationStatusResource resource,
        CancellationToken cancellationToken)
    {
        var command = OperationStatusAssembler.ToCommandFrom(id, resource);
        var result = await operationStatusCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(OperationStatusAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Delete operation status",
        Description = "Deletes an operation telemetry record.",
        OperationId = "DeleteOperation")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Operation status removed successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Operation status not found")]
    public async Task<IActionResult> DeleteOperationAsync(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteOperationStatusCommand(id);
        var result = await operationStatusCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
