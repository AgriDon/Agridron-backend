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
///     HTTP endpoints for farms.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class FarmsController(
    IFarmCommandService farmCommandService,
    IFarmQueryService farmQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Lists farms, optionally filtered by owner.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FarmResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFarmsAsync(
        [FromQuery] long? ownerId,
        [FromQuery] int? page,
        [FromQuery] int? size,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(page, size);
        var result = await farmQueryService.ListAsync(ownerId, pagination, cancellationToken);

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
    [ProducesResponseType(typeof(FarmResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFarmByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await farmQueryService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(FarmAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Registers a farm.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(FarmResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFarmAsync(
        [FromBody] CreateFarmResource resource,
        CancellationToken cancellationToken)
    {
        var result = await farmCommandService.CreateAsync(
            resource.Name,
            resource.Location,
            resource.OwnerId,
            resource.ImageUrl,
            cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var farmResource = FarmAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetFarmById", new { id = farmResource.Id }, farmResource);
    }

    /// <summary>Replaces the data of a farm.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(FarmResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFarmAsync(
        long id,
        [FromBody] UpdateFarmResource resource,
        CancellationToken cancellationToken)
    {
        var result = await farmCommandService.UpdateAsync(
            id,
            resource.Name,
            resource.Location,
            resource.OwnerId,
            resource.ImageUrl,
            cancellationToken);

        return result.IsSuccess
            ? Ok(FarmAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>Deletes a farm that has no parcels.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteFarmAsync(long id, CancellationToken cancellationToken)
    {
        var result = await farmCommandService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }
}
