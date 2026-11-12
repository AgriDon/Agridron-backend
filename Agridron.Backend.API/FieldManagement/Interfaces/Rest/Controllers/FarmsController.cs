using System.Globalization;
using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Controllers;

/// <summary>
///     HTTP endpoints for farms.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Farm Endpoints.")]
public class FarmsController(
    IFarmCommandService farmCommandService,
    IFarmQueryService farmQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists farms, optionally filtered by owner.</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all farms",
        Description = "Retrieves all farms registered in the platform, optionally filtered by owner ID with pagination support.",
        OperationId = "GetFarms")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of farms retrieved successfully", typeof(IEnumerable<FarmResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetFarmsAsync(
        [FromQuery] long? ownerId,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetFarmsByOwnerIdQuery(ownerId, pagination);
        var result = await farmQueryService.Handle(query, cancellationToken);

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

        return Ok(result.Value!.Items.Select(FarmAssembler.ToResourceFrom));
    }

    /// <summary>Finds a farm by its identifier.</summary>
    [HttpGet("{id:long}", Name = "GetFarmById")]
    [SwaggerOperation(
        Summary = "Get farm by ID",
        Description = "Retrieves details of a specific farm by its unique identifier.",
        OperationId = "GetFarmById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Farm found successfully", typeof(FarmResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Farm not found")]
    public async Task<IActionResult> GetFarmByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetFarmByIdQuery(id);
        var result = await farmQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(FarmAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Registers a farm.</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create farm",
        Description = "Registers a new agricultural farm associated with an owner.",
        OperationId = "CreateFarm")]
    [SwaggerResponse(StatusCodes.Status201Created, "Farm created successfully", typeof(FarmResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid farm payload")]
    public async Task<IActionResult> CreateFarmAsync(
        [FromBody] CreateFarmResource resource,
        CancellationToken cancellationToken)
    {
        var command = CreateFarmCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await farmCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var farmResource = FarmAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetFarmById", new { id = farmResource.Id }, farmResource);
    }

    /// <summary>Replaces the data of a farm.</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update farm",
        Description = "Updates the attributes of an existing farm.",
        OperationId = "UpdateFarm")]
    [SwaggerResponse(StatusCodes.Status200OK, "Farm updated successfully", typeof(FarmResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Farm not found")]
    public async Task<IActionResult> UpdateFarmAsync(
        long id,
        [FromBody] UpdateFarmResource resource,
        CancellationToken cancellationToken)
    {
        var command = UpdateFarmCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await farmCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(FarmAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deletes a farm that has no parcels.</summary>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Delete farm",
        Description = "Deletes an agricultural farm as long as it has no active parcels assigned.",
        OperationId = "DeleteFarm")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Farm deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Farm not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Farm still has parcels and cannot be deleted")]
    public async Task<IActionResult> DeleteFarmAsync(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteFarmCommand(id);
        var result = await farmCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
