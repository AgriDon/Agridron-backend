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
///     HTTP endpoints for parcels.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ParcelsController(
    IParcelCommandService parcelCommandService,
    IParcelQueryService parcelQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists parcels, optionally filtered by farm.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ParcelResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetParcelsAsync(
        [FromQuery] long? farmId,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var result = await parcelQueryService.ListAsync(farmId, pagination, cancellationToken);

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
    [ProducesResponseType(typeof(ParcelResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParcelByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await parcelQueryService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(ParcelAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Registers a parcel.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ParcelResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateParcelAsync(
        [FromBody] CreateParcelResource resource,
        CancellationToken cancellationToken)
    {
        var result = await parcelCommandService.CreateAsync(
            resource.FarmId,
            resource.CropId,
            resource.Name,
            resource.Geometry,
            resource.ImageUrl,
            cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var parcelResource = ParcelAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetParcelById", new { id = parcelResource.Id }, parcelResource);
    }

    /// <summary>Replaces the data of a parcel.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ParcelResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateParcelAsync(
        long id,
        [FromBody] UpdateParcelResource resource,
        CancellationToken cancellationToken)
    {
        var result = await parcelCommandService.UpdateAsync(
            id,
            resource.FarmId,
            resource.CropId,
            resource.Name,
            resource.Geometry,
            resource.ImageUrl,
            cancellationToken);

        return result.IsSuccess
            ? Ok(ParcelAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deletes a parcel that has no fumigation areas.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteParcelAsync(long id, CancellationToken cancellationToken)
    {
        var result = await parcelCommandService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
