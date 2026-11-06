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
[SwaggerTag("Available Incident Endpoints.")]
public class IncidentsController(
    IIncidentCommandService incidentCommandService,
    IIncidentQueryService incidentQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all flight incidents",
        Description = "Retrieves telemetry anomalies and flight incidents, optionally filtered by mission, drone, type, or severity.",
        OperationId = "GetIncidents")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of incidents retrieved successfully", typeof(IEnumerable<IncidentResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetIncidentsAsync(
        [FromQuery] long? missionId,
        [FromQuery] long? droneId,
        [FromQuery] string? type,
        [FromQuery] string? severity,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetIncidentsFilteredQuery(missionId, droneId, type, severity, pagination);
        var result = await incidentQueryService.Handle(query, cancellationToken);

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

        return Ok(result.Value!.Items.Select(IncidentAssembler.ToResourceFrom));
    }

    [HttpGet("{id:long}", Name = "GetIncidentById")]
    [SwaggerOperation(
        Summary = "Get incident by ID",
        Description = "Retrieves full details, severity, and timestamps of a specific operational incident by its ID.",
        OperationId = "GetIncidentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Incident found successfully", typeof(IncidentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
    public async Task<IActionResult> GetIncidentByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetIncidentByIdQuery(id);
        var result = await incidentQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(IncidentAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Report flight incident",
        Description = "Reports an anomalous event, obstacle detection, or mechanical failure during mission execution.",
        OperationId = "CreateIncident")]
    [SwaggerResponse(StatusCodes.Status201Created, "Incident reported successfully", typeof(IncidentResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid incident payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Associated mission or drone not found")]
    public async Task<IActionResult> CreateIncidentAsync(
        [FromBody] CreateIncidentResource resource,
        CancellationToken cancellationToken)
    {
        var command = IncidentAssembler.ToCommandFrom(resource);
        var result = await incidentCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var incidentResource = IncidentAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetIncidentById", new { id = incidentResource.Id }, incidentResource);
    }

    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update incident",
        Description = "Updates the description, severity classification, or timestamp of an existing incident.",
        OperationId = "UpdateIncident")]
    [SwaggerResponse(StatusCodes.Status200OK, "Incident updated successfully", typeof(IncidentResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
    public async Task<IActionResult> UpdateIncidentAsync(
        long id,
        [FromBody] UpdateIncidentResource resource,
        CancellationToken cancellationToken)
    {
        var command = IncidentAssembler.ToCommandFrom(id, resource);
        var result = await incidentCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(IncidentAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Delete incident",
        Description = "Removes an incident entry from historical flight telemetry records.",
        OperationId = "DeleteIncident")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Incident removed successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
    public async Task<IActionResult> DeleteIncidentAsync(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteIncidentCommand(id);
        var result = await incidentCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
