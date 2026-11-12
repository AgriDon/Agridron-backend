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
///     HTTP endpoints for parcels.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Parcel Endpoints.")]
public class ParcelsController(
    IParcelCommandService parcelCommandService,
    IParcelQueryService parcelQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists parcels, optionally filtered by farm.</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all parcels",
        Description = "Retrieves active agricultural parcels, optionally filtered by farm ID with pagination support.",
        OperationId = "GetParcels")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of parcels retrieved successfully", typeof(IEnumerable<ParcelResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetParcelsAsync(
        [FromQuery] long? farmId,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetParcelsByFarmIdQuery(farmId, pagination);
        var result = await parcelQueryService.Handle(query, cancellationToken);

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

        return Ok(result.Value!.Items.Select(ParcelAssembler.ToResourceFrom));
    }

    /// <summary>Finds a parcel by its identifier.</summary>
    [HttpGet("{id:long}", Name = "GetParcelById")]
    [SwaggerOperation(
        Summary = "Get parcel by ID",
        Description = "Retrieves details and boundary polygon of a specific parcel by its ID.",
        OperationId = "GetParcelById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Parcel found successfully", typeof(ParcelResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Parcel not found or inactive")]
    public async Task<IActionResult> GetParcelByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetParcelByIdQuery(id);
        var result = await parcelQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(ParcelAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Registers a parcel.</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create parcel",
        Description = "Registers a new parcel within a farm, deriving its area from the provided GeoJSON polygon and recording optional phenological stage.",
        OperationId = "CreateParcel")]
    [SwaggerResponse(StatusCodes.Status201Created, "Parcel created successfully", typeof(ParcelResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid parcel payload or degenerate geometry")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Associated farm or crop not found")]
    public async Task<IActionResult> CreateParcelAsync(
        [FromBody] CreateParcelResource resource,
        CancellationToken cancellationToken)
    {
        var command = CreateParcelCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await parcelCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var parcelResource = ParcelAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetParcelById", new { id = parcelResource.Id }, parcelResource);
    }

    /// <summary>Replaces the data of a parcel.</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update parcel",
        Description = "Updates the attributes, boundary polygon or phenological stage of an existing parcel.",
        OperationId = "UpdateParcel")]
    [SwaggerResponse(StatusCodes.Status200OK, "Parcel updated successfully", typeof(ParcelResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Parcel, farm, or crop not found")]
    public async Task<IActionResult> UpdateParcelAsync(
        long id,
        [FromBody] UpdateParcelResource resource,
        CancellationToken cancellationToken)
    {
        var command = UpdateParcelCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await parcelCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(ParcelAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deactivates a parcel (soft delete).</summary>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Deactivate parcel",
        Description = "Inactivates an agricultural parcel (soft delete) to retire it from active operations while maintaining flight mission history.",
        OperationId = "DeleteParcel")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Parcel deactivated successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Parcel not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Parcel still has active fumigation areas")]
    public async Task<IActionResult> DeleteParcelAsync(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteParcelCommand(id);
        var result = await parcelCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
