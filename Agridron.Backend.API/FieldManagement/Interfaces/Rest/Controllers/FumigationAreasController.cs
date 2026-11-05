using System.Globalization;
using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Controllers;

/// <summary>
///     HTTP endpoints for fumigation areas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class FumigationAreasController(
    IFumigationAreaCommandService fumigationAreaCommandService,
    IFumigationAreaQueryService fumigationAreaQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists fumigation areas, optionally filtered by parcel.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FumigationAreaResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFumigationAreasAsync(
        [FromQuery] long? parcelId,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var result = await fumigationAreaQueryService.ListAsync(parcelId, pagination, cancellationToken);

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
    [ProducesResponseType(typeof(FumigationAreaResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFumigationAreaByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await fumigationAreaQueryService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(FumigationAreaAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Defines a fumigation area inside a parcel.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(FumigationAreaResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateFumigationAreaAsync(
        [FromBody] CreateFumigationAreaResource resource,
        CancellationToken cancellationToken)
    {
        var result = await fumigationAreaCommandService.CreateAsync(
            resource.ParcelId,
            resource.Geometry,
            cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var areaResource = FumigationAreaAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetFumigationAreaById", new { id = areaResource.Id }, areaResource);
    }

    /// <summary>Redefines the polygon of a fumigation area.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(FumigationAreaResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateFumigationAreaAsync(
        long id,
        [FromBody] UpdateFumigationAreaResource resource,
        CancellationToken cancellationToken)
    {
        var result = await fumigationAreaCommandService.UpdateAsync(
            id,
            resource.ParcelId,
            resource.Geometry,
            cancellationToken);

        return result.IsSuccess
            ? Ok(FumigationAreaAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deletes a fumigation area.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFumigationAreaAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await fumigationAreaCommandService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
