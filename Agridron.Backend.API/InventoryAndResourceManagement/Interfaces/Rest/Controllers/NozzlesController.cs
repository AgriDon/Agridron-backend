using Agridron.Backend.API.InventoryAndResourceManagement.Application.CommandServices;
using Agridron.Backend.API.InventoryAndResourceManagement.Application.QueryServices;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Commands;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Queries;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Resources;
using Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Transform;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Nozzle (spray nozzle inventory) Endpoints.")]
public class NozzlesController(
    INozzleCommandService nozzleCommandService,
    INozzleQueryService nozzleQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all nozzles",
        Description = "Retrieves the active spray nozzles of the inventory, optionally filtered by status ('good' or 'worn').",
        OperationId = "GetNozzles")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of nozzles retrieved successfully", typeof(IEnumerable<NozzleResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid nozzle status")]
    public async Task<IActionResult> GetNozzlesAsync([FromQuery] string? status, CancellationToken cancellationToken)
    {
        NozzleStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!NozzleAssembler.TryParseStatus(status, out var parsedStatus))
            {
                return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.NozzleStatusInvalid);
            }

            statusFilter = parsedStatus;
        }

        var result = await nozzleQueryService.Handle(new GetAllNozzlesQuery(statusFilter), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value!.Select(NozzleAssembler.ToResourceFrom))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpGet("{id:long}", Name = "GetNozzleById")]
    [SwaggerOperation(
        Summary = "Get nozzle by ID",
        Description = "Retrieves an active spray nozzle by its ID.",
        OperationId = "GetNozzleById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Nozzle found successfully", typeof(NozzleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Nozzle not found or inactive")]
    public async Task<IActionResult> GetNozzleByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await nozzleQueryService.Handle(new GetNozzleByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(NozzleAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Register nozzle",
        Description = "Registers spray nozzles with their type, diameter, units and condition (DR-004).",
        OperationId = "CreateNozzle")]
    [SwaggerResponse(StatusCodes.Status201Created, "Nozzle registered successfully", typeof(NozzleResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid nozzle payload")]
    public async Task<IActionResult> CreateNozzleAsync(
        [FromBody] CreateNozzleResource resource,
        CancellationToken cancellationToken)
    {
        if (!NozzleAssembler.TryParseType(resource.Type, out var type))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.NozzleTypeInvalid);
        }

        if (!NozzleAssembler.TryParseStatus(resource.Status, out var status))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.NozzleStatusInvalid);
        }

        var command = new CreateNozzleCommand(type, resource.DiameterMm, resource.StockUnits, status);
        var result = await nozzleCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var nozzleResource = NozzleAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetNozzleById", new { id = nozzleResource.Id }, nozzleResource);
    }

    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update nozzle",
        Description = "Updates the type, diameter, units or condition of a spray nozzle.",
        OperationId = "UpdateNozzle")]
    [SwaggerResponse(StatusCodes.Status200OK, "Nozzle updated successfully", typeof(NozzleResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid nozzle payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Nozzle not found")]
    public async Task<IActionResult> UpdateNozzleAsync(
        long id,
        [FromBody] UpdateNozzleResource resource,
        CancellationToken cancellationToken)
    {
        if (!NozzleAssembler.TryParseType(resource.Type, out var type))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.NozzleTypeInvalid);
        }

        if (!NozzleAssembler.TryParseStatus(resource.Status, out var status))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.NozzleStatusInvalid);
        }

        var command = new UpdateNozzleCommand(id, type, resource.DiameterMm, resource.StockUnits, status);
        var result = await nozzleCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(NozzleAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Delete nozzle",
        Description = "Soft delete: the nozzle leaves the inventory but stays in the database.",
        OperationId = "DeleteNozzle")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Nozzle deactivated successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Nozzle not found")]
    public async Task<IActionResult> DeleteNozzleAsync(long id, CancellationToken cancellationToken)
    {
        var result = await nozzleCommandService.Handle(new DeleteNozzleCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost("{id:long}/toggle-status")]
    [SwaggerOperation(
        Summary = "Toggle nozzle condition",
        Description = "Switches the nozzle condition between 'good' and 'worn'.",
        OperationId = "ToggleNozzleStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Nozzle condition changed", typeof(NozzleResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Nozzle not found")]
    public async Task<IActionResult> ToggleNozzleStatusAsync(long id, CancellationToken cancellationToken)
    {
        var result = await nozzleCommandService.Handle(new ToggleNozzleStatusCommand(id), cancellationToken);

        return result.IsSuccess
            ? Ok(NozzleAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
