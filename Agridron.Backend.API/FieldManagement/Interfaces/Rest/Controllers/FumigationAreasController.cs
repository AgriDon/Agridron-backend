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
///     HTTP endpoints for fumigation areas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Fumigation Area Endpoints.")]
public class FumigationAreasController(
    IFumigationAreaCommandService fumigationAreaCommandService,
    IFumigationAreaQueryService fumigationAreaQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists fumigation areas, optionally filtered by parcel.</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all fumigation areas",
        Description = "Retrieves all defined fumigation areas, optionally filtered by parcel ID with pagination support.",
        OperationId = "GetFumigationAreas")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of fumigation areas retrieved successfully", typeof(IEnumerable<FumigationAreaResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetFumigationAreasAsync(
        [FromQuery] long? parcelId,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetFumigationAreasByParcelIdQuery(parcelId, pagination);
        var result = await fumigationAreaQueryService.Handle(query, cancellationToken);

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

        return Ok(result.Value!.Items.Select(FumigationAreaAssembler.ToResourceFrom));
    }

    /// <summary>Finds a fumigation area by its identifier.</summary>
    [HttpGet("{id:long}", Name = "GetFumigationAreaById")]
    [SwaggerOperation(
        Summary = "Get fumigation area by ID",
        Description = "Retrieves details and boundary polygon of a specific fumigation area by its ID.",
        OperationId = "GetFumigationAreaById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Fumigation area found successfully", typeof(FumigationAreaResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Fumigation area not found")]
    public async Task<IActionResult> GetFumigationAreaByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var query = new GetFumigationAreaByIdQuery(id);
        var result = await fumigationAreaQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(FumigationAreaAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Defines a fumigation area inside a parcel.</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Define fumigation area",
        Description = "Defines a sub-polygon representing the specific zone to be fumigated inside an active parcel.",
        OperationId = "CreateFumigationArea")]
    [SwaggerResponse(StatusCodes.Status201Created, "Fumigation area defined successfully", typeof(FumigationAreaResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid payload or degenerate geometry")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Associated parcel not found or inactive")]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Fumigation area exceeds parcel boundary area")]
    public async Task<IActionResult> CreateFumigationAreaAsync(
        [FromBody] CreateFumigationAreaResource resource,
        CancellationToken cancellationToken)
    {
        var command = CreateFumigationAreaCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await fumigationAreaCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var areaResource = FumigationAreaAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetFumigationAreaById", new { id = areaResource.Id }, areaResource);
    }

    /// <summary>Redefines the polygon of a fumigation area.</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Redefine fumigation area",
        Description = "Redefines the boundary polygon of an existing fumigation area.",
        OperationId = "UpdateFumigationArea")]
    [SwaggerResponse(StatusCodes.Status200OK, "Fumigation area updated successfully", typeof(FumigationAreaResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Fumigation area or parcel not found")]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Fumigation area exceeds parcel boundary area")]
    public async Task<IActionResult> UpdateFumigationAreaAsync(
        long id,
        [FromBody] UpdateFumigationAreaResource resource,
        CancellationToken cancellationToken)
    {
        var command = UpdateFumigationAreaCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await fumigationAreaCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(FumigationAreaAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deletes a fumigation area.</summary>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Delete fumigation area",
        Description = "Deletes an existing fumigation area.",
        OperationId = "DeleteFumigationArea")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Fumigation area deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Fumigation area not found")]
    public async Task<IActionResult> DeleteFumigationAreaAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFumigationAreaCommand(id);
        var result = await fumigationAreaCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
