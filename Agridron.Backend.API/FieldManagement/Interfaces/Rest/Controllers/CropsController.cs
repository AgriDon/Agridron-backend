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
///     HTTP endpoints for crops.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Crop Endpoints.")]
public class CropsController(
    ICropCommandService cropCommandService,
    ICropQueryService cropQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists every crop.</summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all crops",
        Description = "Retrieves all agricultural crop types and varieties registered in the system with pagination support.",
        OperationId = "GetCrops")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of crops retrieved successfully", typeof(IEnumerable<CropResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameters")]
    public async Task<IActionResult> GetCropsAsync(
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var query = new GetAllCropsQuery(pagination);
        var result = await cropQueryService.Handle(query, cancellationToken);

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

        return Ok(result.Value!.Items.Select(CropAssembler.ToResourceFrom));
    }

    /// <summary>Finds a crop by its identifier.</summary>
    [HttpGet("{id:long}", Name = "GetCropById")]
    [SwaggerOperation(
        Summary = "Get crop by ID",
        Description = "Retrieves details of a specific crop by its unique identifier.",
        OperationId = "GetCropById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Crop found successfully", typeof(CropResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Crop not found")]
    public async Task<IActionResult> GetCropByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = new GetCropByIdQuery(id);
        var result = await cropQueryService.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(CropAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Registers a crop.</summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create crop",
        Description = "Registers a new crop with its name and variety combination.",
        OperationId = "CreateCrop")]
    [SwaggerResponse(StatusCodes.Status201Created, "Crop registered successfully", typeof(CropResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid crop payload")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Crop with same name and variety already exists")]
    public async Task<IActionResult> CreateCropAsync(
        [FromBody] CreateCropResource resource,
        CancellationToken cancellationToken)
    {
        var command = CreateCropCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await cropCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var cropResource = CropAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetCropById", new { id = cropResource.Id }, cropResource);
    }

    /// <summary>Updates a crop.</summary>
    [HttpPut("{id:long}")]
    [SwaggerOperation(
        Summary = "Update crop",
        Description = "Updates the name or variety of an existing crop.",
        OperationId = "UpdateCrop")]
    [SwaggerResponse(StatusCodes.Status200OK, "Crop updated successfully", typeof(CropResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid update payload")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Crop not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Crop with same name and variety already exists")]
    public async Task<IActionResult> UpdateCropAsync(
        long id,
        [FromBody] UpdateCropResource resource,
        CancellationToken cancellationToken)
    {
        var command = UpdateCropCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await cropCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(CropAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deletes a crop that is not assigned to any parcel.</summary>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(
        Summary = "Delete crop",
        Description = "Deletes a crop as long as it is not assigned to any active parcel.",
        OperationId = "DeleteCrop")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Crop deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Crop not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Crop is assigned to parcels and cannot be deleted")]
    public async Task<IActionResult> DeleteCropAsync(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteCropCommand(id);
        var result = await cropCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
