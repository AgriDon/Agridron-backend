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
///     HTTP endpoints for crops.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CropsController(
    ICropCommandService cropCommandService,
    ICropQueryService cropQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists every crop.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CropResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCropsAsync(
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var result = await cropQueryService.ListAsync(pagination, cancellationToken);

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
    [ProducesResponseType(typeof(CropResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCropByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await cropQueryService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(CropAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Registers a crop.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CropResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCropAsync(
        [FromBody] CreateCropResource resource,
        CancellationToken cancellationToken)
    {
        var result = await cropCommandService.CreateAsync(
            resource.Name,
            resource.Variety,
            cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var cropResource = CropAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetCropById", new { id = cropResource.Id }, cropResource);
    }

    /// <summary>Updates a crop.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(CropResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCropAsync(
        long id,
        [FromBody] UpdateCropResource resource,
        CancellationToken cancellationToken)
    {
        var result = await cropCommandService.UpdateAsync(
            id,
            resource.Name,
            resource.Variety,
            cancellationToken);

        return result.IsSuccess
            ? Ok(CropAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deletes a crop that is not assigned to any parcel.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCropAsync(long id, CancellationToken cancellationToken)
    {
        var result = await cropCommandService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
