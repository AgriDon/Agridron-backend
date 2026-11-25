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
[SwaggerTag("Available Chemical Endpoints.")]
public class ChemicalsController(
    IChemicalCommandService chemicalCommandService,
    IChemicalQueryService chemicalQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all chemicals",
        Description = "Retrieves the active agrochemicals of the inventory, optionally filtered by type.",
        OperationId = "GetChemicals")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of chemicals retrieved successfully", typeof(IEnumerable<ChemicalResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid chemical type")]
    public async Task<IActionResult> GetChemicalsAsync([FromQuery] string? type, CancellationToken cancellationToken)
    {
        ChemicalType? typeFilter = null;
        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!ChemicalAssembler.TryParseType(type, out var parsedType))
            {
                return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.ChemicalTypeInvalid);
            }

            typeFilter = parsedType;
        }

        var result = await chemicalQueryService.Handle(new GetAllChemicalsQuery(typeFilter), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value!.Select(ChemicalAssembler.ToResourceFrom))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpGet("{id:long}", Name = "GetChemicalById")]
    [SwaggerOperation(
        Summary = "Get chemical by ID",
        Description = "Retrieves an active agrochemical by its ID.",
        OperationId = "GetChemicalById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Chemical found successfully", typeof(ChemicalResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Chemical not found or inactive")]
    public async Task<IActionResult> GetChemicalByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await chemicalQueryService.Handle(new GetChemicalByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ChemicalAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Register chemical",
        Description = "Registers a new agrochemical with its type, stock, minimum stock and expiration date (DR-003).",
        OperationId = "CreateChemical")]
    [SwaggerResponse(StatusCodes.Status201Created, "Chemical registered successfully", typeof(ChemicalResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid chemical payload")]
    public async Task<IActionResult> CreateChemicalAsync(
        [FromBody] CreateChemicalResource resource,
        CancellationToken cancellationToken)
    {
        if (!ChemicalAssembler.TryParseType(resource.Type, out var type))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.ChemicalTypeInvalid);
        }

        if (!ChemicalAssembler.TryParseExpirationDate(resource.ExpirationDate, out var expirationDate))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.ChemicalExpirationDateRequired);
        }

        var command = new CreateChemicalCommand(
            resource.Name,
            type,
            resource.StockLiters,
            resource.MinimumStockLiters,
            expirationDate);

        var result = await chemicalCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var chemicalResource = ChemicalAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetChemicalById", new { id = chemicalResource.Id }, chemicalResource);
    }

    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update chemical",
        Description = "Updates the name, type, stock, minimum stock or expiration date of an agrochemical.",
        OperationId = "UpdateChemical")]
    [SwaggerResponse(StatusCodes.Status200OK, "Chemical updated successfully", typeof(ChemicalResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid chemical payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Chemical not found")]
    public async Task<IActionResult> UpdateChemicalAsync(
        long id,
        [FromBody] UpdateChemicalResource resource,
        CancellationToken cancellationToken)
    {
        if (!ChemicalAssembler.TryParseType(resource.Type, out var type))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.ChemicalTypeInvalid);
        }

        if (!ChemicalAssembler.TryParseExpirationDate(resource.ExpirationDate, out var expirationDate))
        {
            return problemDetailsFactory.CreateProblemDetails(this, InventoryErrors.ChemicalExpirationDateRequired);
        }

        var command = new UpdateChemicalCommand(
            id,
            resource.Name,
            type,
            resource.StockLiters,
            resource.MinimumStockLiters,
            expirationDate);

        var result = await chemicalCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ChemicalAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Delete chemical",
        Description = "Soft delete: the chemical leaves the inventory but stays in the database for history and reports.",
        OperationId = "DeleteChemical")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Chemical deactivated successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Chemical not found")]
    public async Task<IActionResult> DeleteChemicalAsync(long id, CancellationToken cancellationToken)
    {
        var result = await chemicalCommandService.Handle(new DeleteChemicalCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }


    ///Controller to restock the chemical stock

    [HttpPost("{id:long}/restock")]
    [SwaggerOperation(
        Summary = "Restock chemical",
        Description = "Adds liters to the current stock of an agrochemical. The amount must be greater than zero.",
        OperationId = "RestockChemical")]
    [SwaggerResponse(StatusCodes.Status200OK, "Chemical restocked successfully", typeof(ChemicalResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The restock amount must be greater than zero")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Chemical not found")]
    public async Task<IActionResult> RestockChemicalAsync(
        long id,
        [FromBody] RestockChemicalResource resource,
        CancellationToken cancellationToken)
    {
        var command = new RestockChemicalCommand(id, resource.Liters);
        var result = await chemicalCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ChemicalAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

}
